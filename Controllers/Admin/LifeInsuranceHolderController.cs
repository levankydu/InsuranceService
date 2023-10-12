using Microsoft.AspNetCore.Mvc;
using test0000001.Extensions;
using test0000001.Models.DTO.LifeInsurance;
using test0000001.Repository.ServiceClass.LifeInsurance;

namespace test0000001.Controllers.Admin
{
    using Microsoft.AspNetCore.Authorization;
    using test0000001.Models.LifeInsurance;

    [Route("Admin/[Controller]/[Action]/{id?}")]
    [Authorize(Roles = "admin")]
    public class LifeInsuranceHolderController : Controller
    {
        private readonly PolicyHolderViewService _holderView;
        private readonly PolicyHolderService _policyHolder;
        private readonly PaymentScheduleService _schedule;
        private readonly MailingService _mailing;
        private readonly PdfHandlerService _pdf;
        private readonly string _viewPath = "../Admin/LifeInsurance/PolicyHolder";

        public LifeInsuranceHolderController(
            PolicyHolderViewService holderView,
            PolicyHolderService policyHolder,
            PaymentScheduleService schedule,
            MailingService mailing,
            PdfHandlerService pdf)
        {
            _holderView = holderView;
            _policyHolder = policyHolder;
            _schedule = schedule;
            _mailing = mailing;
            _pdf = pdf;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var model = _policyHolder.GetAll().OrderByDescending(m => m.Status == "Pending");
            ViewBag.DueCounts = _schedule.GetDueCounts();
            return View($"{_viewPath}/Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                var model = await _holderView.GetPackageDetail(id);
                if (model == null) return NotFound();

                ViewBag.IsAdminPage = true;
                ViewBag.DescCollapsable = true;
                return View($"{_viewPath}/Details", model);
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var approveResult = _policyHolder.ApproveById(id);
                if (!approveResult) throw new Exception("Something bad happened in dataset");

                var packageItms = await _holderView.GetPackageDetail(id)
                    ?? throw new Exception("Cannot get packageItms information");
                await MakePaymentSchedule(packageItms!);

                ViewBag.DescCollapsable = false;
                var pdfContent = new ContractContent
                {
                    Header = await this.RenderViewAsync("../LifeInsurance/_ContractHeader", packageItms),
                    Footer = await this.RenderViewAsync("../LifeInsurance/_ContractFooter", packageItms),
                    Body = await this.RenderViewAsync("../LifeInsurance/PackageRegisterToPdfApproved", packageItms)
                };
                string fileName = $"contracts/LifeInsurance/PackageRegistration_{packageItms?.InsuredObject.AppraisalManifestId}_{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}.pdf";
                var pdfPath = await _pdf.HtmlToPdf(pdfContent, fileName)
                    ?? throw new Exception("Cannot create pdf file.");
                var pwd = $"{packageItms?.User.FirstName}@{packageItms?.User.DateOfBirth.ToString("ddMM")}";
                pdfPath = _pdf.AddPassword(pdfPath, pwd);

                await Mailing("kientvts2204051@fpt.edu.vn", "ptge rpvw hgew mnab", 
                    packageItms!, Status.Activated, pdfPath, pdfPassword: pwd);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            try
            {
                var rejectResult = _policyHolder.RejectById(id);
                if (!rejectResult) throw new Exception("Something bad happened in dataset");

                var packageItms = await _holderView.GetPackageDetail(id)
                    ?? throw new Exception("Cannot get PackageItms information");

                await Mailing("kientvts2204051@fpt.edu.vn", "ptge rpvw hgew mnab", 
                    packageItms, Status.Rejected, reason: reason);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Terminate(int id, string reason)
        {
            try
            {
                var rejectResult = _policyHolder.TerminateById(id);
                if (!rejectResult) throw new Exception("Something bad happened in dataset");

                var packageItms = await _holderView.GetPackageDetail(id)
                    ?? throw new Exception("Cannot get PackageItms information");

                await Mailing("kientvts2204051@fpt.edu.vn", "ptge rpvw hgew mnab",
                    packageItms, Status.Terminated, reason: reason);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        public async Task MakePaymentSchedule(PackageOverviewDto packageItms)
        {
            try
            {
                var term = (packageItms.Package?.Duration?.Term) ?? throw new Exception("Cannot get Term information");
                var terms = term / 12;
                var amount = packageItms?.Package?.Premium / terms;
                var userId = packageItms?.User.Id;
                var policyId = packageItms?.PolicyHolder.Id;
                var firstDueDate = packageItms?.PolicyHolder.StartDay.AddDays(20);
                if (amount == null || userId == null ||
                    policyId == null || firstDueDate == null)
                    throw new Exception("Cannot get Detail information");

                var schedules = new List<PaymentSchedule>();
                for (var i = 0; i < terms; i++)
                {
                    schedules.Add(new PaymentSchedule
                    {
                        UserId = userId,
                        PolicyHolderId = (int)policyId,
                        Amount = (decimal)amount,
                        Description = $"Settlement period of {i + 1} out of {terms}",
                        Status = PaymentStatus.NotDue,
                        DueDate = firstDueDate.Value.AddYears(i)
                    });
                }
                await Task.FromResult(_schedule.AddRange(schedules));
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task Mailing(
            string email,
            string password,
            PackageOverviewDto packageItms,
            Status status,
            string? attachment = null,
            string? pdfPassword = null,
            string? reason = null)
        {
            try
            {
                string subject = status switch
                {
                    Status.Activated => "APPROVED",
                    Status.Rejected => "REJECTED",
                    Status.Terminated => "TERMINATED",
                    _ => ""
                };
                string template = status switch
                {
                    Status.Activated => "../LifeInsurance/MailingPackageApproved",
                    Status.Rejected => "../LifeInsurance/MailingPackageRejected",
                    Status.Terminated => "../LifeInsurance/MailingPackageTerminated",
                    _ => ""
                };
                ViewBag.Reason = reason;
                ViewBag.PdfPassword = pdfPassword;

                var body = await this.RenderViewAsync(template, packageItms);
                var mail = new MailingData
                {
                    From = email,
                    To = packageItms!.InsuredObject.Email,
                    Password = password,
                    Subject = "InsuranceService :: Registered package " + subject,
                    Body = body,
                    FilePath = attachment
                };
                _mailing.SendMail(mail);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}

