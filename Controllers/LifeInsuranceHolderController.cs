using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using test0000001.Clients;
using test0000001.Extensions;
using test0000001.Models;
using test0000001.Models.DTO.LifeInsurance;
using test0000001.Models.LifeInsurance;
using test0000001.Repository.ServiceClass.LifeInsurance;

namespace test0000001.Controllers
{
    [Authorize(Roles = "user")]
    public class LifeInsuranceHolderController : Controller
    {
        private readonly UserManager<ApplicationUser> _usrMgr;
        private readonly PolicyHolderViewService _holderView;
        private readonly PolicyHolderService _policyHolder;
        private readonly PaymentScheduleService _schedule;
        private readonly PaypalForLifeClient _paypalClient;
        private readonly MailingService _mailing;
        private readonly string _viewPath = "../LifeInsurance/PolicyHolder";

        public LifeInsuranceHolderController(
            UserManager<ApplicationUser> usrMgr,
            PolicyHolderViewService holderView,
            PolicyHolderService policyHolder,
            PaymentScheduleService schedule,
            PaypalForLifeClient paypalClient,
            MailingService mailing
            )
        {
            _usrMgr = usrMgr;
            _holderView = holderView;
            _policyHolder = policyHolder;
            _schedule = schedule;
            _paypalClient = paypalClient;
            _mailing = mailing;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await GetSignedInUser();
                if (user == null) return NotFound();
                var model = await Task.FromResult(_policyHolder.GetByUserId(user.Id));
                ViewBag.DueCounts = _schedule.GetDueCountsByUserId(user.Id);
                return View($"{_viewPath}/Index", model);
			}
            catch (Exception e)
            {
				var error = new { e.GetBaseException().Message };
				return BadRequest(error);
			}
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                var model = await _holderView.GetPackageDetail(id);
                if (model == null) return NotFound();

                ViewBag.Package = model.Package;
                ViewBag.IsAdminPage = false;
                ViewBag.DescCollapsable = true;
                return View($"{_viewPath}/Details", model);
            }
            catch (Exception e)
            {
				var error = new { e.GetBaseException().Message };
				return BadRequest(error);
			}
        }

        [HttpGet]
        [Route("[Controller]/{packageId}/[Action]/{id?}")]
        public async Task<IActionResult> Payment(int? id, int? packageId)
        {
            try
            {
                if (id == null || packageId == null) return NotFound();
                var model = await GetPaymentDto(id.Value, packageId.Value);
                if (model == null) return NotFound();

                // ViewBag.ClientId is used to get the Paypal Checkout javascript SDK
                ViewBag.ClientId = _paypalClient.ClientId;
                return View($"{_viewPath}/Paypal", model);
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpPost]
        [Route("[Controller]/{packageId}/[Action]/{id?}")]
        public async Task<IActionResult> PaypalOrder(int id, int packageId, CancellationToken cancellationToken)
        {
            try
            {
                var model = await GetPaymentDto(id, packageId);
                if (model == null) return NotFound();

                // set the transaction price and currency
                var price = model.PaidItem!.Amount.ToString();
                var currency = "USD";

                // "reference" is the transaction key
                var reference = $"{model.PackageOverview!.InsuredObject.AppraisalManifestId}_SettlementNo_{id}";
                var response = await _paypalClient.CreateOrder(price, currency, reference);

                return Ok(response);
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpPost]
        [Route("[Controller]/{packageId}/[Action]/{id?}")]
        public async Task<IActionResult> PaypalCapture(int id, int packageId, string orderId)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderId);
                var reference = response.purchase_units![0].reference_id;

                // Put your logic to save the transaction here
                // You can use the "reference" variable as a transaction key
                var model = await GetPaymentDto(id, packageId);
                if (model == null) return NotFound();

                var payment = new Payment
                {
                    CreatedAt = DateTime.Now,
                    PolicyholderId = model.PackageOverview!.PolicyHolder.Id,
                    Amount = model.PaidItem!.Amount,
                };

                await Task.FromResult(_schedule.AddPayment(payment));
                await Task.FromResult(_schedule.SetPaymentToPaid(id, payment.Id));
                model.PaidItem = _schedule.GetById(id, asNoTracking: true);
                //TempData.Put("PaypalPaymentDto", model);
                _schedule.SetPaypalPaymentObject(id, packageId, model);

                return Ok(response);
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpGet]
        [Route("[Controller]/{packageId}/[Action]/{id?}")]
        public async Task<IActionResult> PaypalSuccess(int id, int packageId)
        {
            //var model = TempData.Get<PaypalPaymentDto>("PaypalPaymentDto");
            var model = _schedule.GetPaypalPaymentObject(id, packageId);
            if (model == null) return NotFound();

            model.Payment = _schedule.GetPaymentById(model.PaidItem!.PaymentId!.Value, asNoTracking: true);
            model.NextPaidItem = _schedule.GetNextPayment(model.PolicyHolderId, asNoTracking: true);

            await Mailing("kientvts2204051@fpt.edu.vn", "ptge rpvw hgew mnab", model);
            _schedule.ClearPaypalPaymentObject(id, packageId);
            return View($"{_viewPath}/PaymentSuccess", model);
        }


        private async Task<IActionResult> PaymentSucceed(int id, int packageId)
        {
            try
            {
                var model = await GetPaymentDto(id, packageId);
                if (model == null) return NotFound();

                model.Payment = _schedule.GetPaymentById(model.PaidItem!.PaymentId!.Value, asNoTracking: true);
                model.NextPaidItem = _schedule.GetNextPayment(packageId);

                return View($"{_viewPath}/PaymentSuccess", model);
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        
        private async Task<IActionResult> MailingPaymentSucceed(int id, int packageId)
        {
            try
            {
                var model = await GetPaymentDto(id, packageId);
                if (model == null) return NotFound();

                model.Payment = _schedule.GetPaymentById(model.PaidItem!.PaymentId!.Value, asNoTracking: true);
                model.NextPaidItem = _schedule.GetNextPayment(packageId);

                return View($"{_viewPath}/MailingPaymentSuccess", model);
            }
            catch (Exception e)
            {
                var error = new { e.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        private async Task Mailing(string email, string password, PaypalPaymentDto model)
        {
            try
            {
                var body = await this.RenderViewAsync($"{_viewPath}/MailingPaymentSuccess", model);
                var mail = new MailingData
                {
                    From = email,
                    To = model.PackageOverview!.InsuredObject.Email,
                    Password = password,
                    Subject = "InsuranceService :: Payment succeeded",
                    Body = body
                };
                _mailing.SendMail(mail);

            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<PaypalPaymentDto?> GetPaymentDto(int id, int packageId)
        {
            try
            {
                var holder = await _holderView.GetPackageDetail(packageId);
                if (holder == null) return null;

                var paidItm = _schedule.GetById(id, asNoTracking: true);
                if (paidItm == null) return null;

                return new PaypalPaymentDto
                {
                    PolicyHolderId = packageId,
                    PaymentScheduleId = id,
                    PackageOverview = holder,
                    PaidItem = paidItm
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<ApplicationUser?> GetSignedInUser()
        {
            string username = User.Identity?.Name ?? string.Empty;
            if (string.IsNullOrEmpty(username)) return null;
            return await _usrMgr.FindByNameAsync(username);
        }
    }
}
