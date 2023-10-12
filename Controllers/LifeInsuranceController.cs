using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using test0000001.DB;
using test0000001.Extensions;
using test0000001.Models;
using test0000001.Models.DTO.LifeInsurance;
using test0000001.Models.LifeInsurance;
using test0000001.Repository.ServiceClass.LifeInsurance;

namespace test0000001.Controllers
{
    [Authorize(Roles = "user")]
    public class LifeInsuranceController : Controller
    {
        private readonly UserManager<ApplicationUser> _usrMgr;
        private readonly PolicyHolderService _policyHolder;
        private readonly LifeInsuranceService _lifeInsurance;
        private readonly LifeInsuredObjectService _lifeInsuredObj;
        private readonly LifeInsuredDossiersService _lifeInsuredDossier;
        private readonly AppraisalInfoService _appraisalInfo;
        private readonly AppraisalQuesService _appraisalQues;
        private readonly FileHandlerService _fileHandler;
        private readonly MailingService _mailing;
        private readonly PdfHandlerService _pdf;
        private readonly DatabaseContext _dbContext;

        public LifeInsuranceController(
            UserManager<ApplicationUser> usrMgr,
            PolicyHolderService policyHolder,
            LifeInsuranceService lifeInsurance,
            LifeInsuredObjectService lifeInsuredObj,
            LifeInsuredDossiersService lifeInsuredDossier,
            AppraisalInfoService appraisalInfo,
            AppraisalQuesService appraisalQues,
            FileHandlerService fileHandler,
            MailingService mailing,
            PdfHandlerService pdf,
            DatabaseContext dbContext)
        {
            _usrMgr = usrMgr;
            _policyHolder = policyHolder;
            _lifeInsurance = lifeInsurance;
            _lifeInsuredObj = lifeInsuredObj;
            _lifeInsuredDossier = lifeInsuredDossier;
            _appraisalInfo = appraisalInfo;
            _appraisalQues = appraisalQues;
            _fileHandler = fileHandler;
            _mailing = mailing;
            _pdf = pdf;
            _dbContext = dbContext;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var model = _lifeInsurance.GetAll();
            return View(model);
        }

        [AllowAnonymous]
        [Route("[Controller]/{slug}")]
        public IActionResult Details(string slug)
        {
            var model = _lifeInsurance.GetBySlug(slug);
            return View(model);
        }

        [Route("[Controller]/{slug}/[Action]")]
        [ActionName("PackageRegister")]
        public IActionResult CreateInsuredObject(string slug)
        {
            ViewBag.Package = _lifeInsurance.SetPackage(slug);
            var policyHolder = _policyHolder.GetPolicyHolder(slug);
            var insuredObj = _lifeInsuredObj.GetInsuredObject(slug);

            return policyHolder == null || insuredObj == null ?
                View("CreateInsuredObject") :
                View("CreateInsuredObject", new InsuredObjectDto
                {
                    PolicyHolder = policyHolder,
                    InsuredObject = insuredObj
                });
        }

        [HttpPost]
        [Route("[Controller]/{slug}/[Action]")]
        [ActionName("PackageRegister")]
        public async Task<IActionResult> ProcessInsuredObject(string slug, InsuredObjectDto obj)
        {
            try
            {
                var package = _lifeInsurance.GetPackage(slug);
                if (package == null) return BadRequest();
                ViewBag.Package = package;

                var duration = package.Duration?.Term ?? 0;
                var user = await GetSignedInUser();

                ModelState.Clear();
                obj.PolicyHolder.UserId = user?.Id;
                obj.PolicyHolder.EndDay = obj.PolicyHolder.StartDay.AddMonths(duration);
                obj.PolicyHolder.PolicyId = package.Id;
                obj.PolicyHolder.Status = "Pending";
                //obj.PolicyHolder.IsActive = false;

                if (TryValidateModel(obj))
                {
                    _policyHolder.SetPolicyHolder(slug, obj.PolicyHolder);
                    _lifeInsuredObj.SetInsuredObject(slug, obj.InsuredObject);
                    return CreateAppraisalInfos(slug);
                }
                return View("CreateInsuredObject", obj);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View("CreateInsuredObject", obj);
            }
        }

        [HttpPost]
        [Route("[Controller]/{slug}/[Action]")]
        [ActionName("PackageRegister-Appraisal-View")]
        public IActionResult CreateAppraisalInfos(string slug)
        {
            ViewBag.Package = _lifeInsurance.GetPackage(slug);

            var model = _appraisalInfo.GetAppraisalInfos(slug);
            if (model != null) return View("CreateAppraisalInfos", model);

            var ques = _appraisalQues.GetByCateId(1);
            model = new AppraisalInfosDto();
            var infos = ques.Select(q => new AppraisalInfo
            {
                Description = q.Description,
                DescriptionType = q.DescriptionType,
                DescriptionDetail = q.DescriptionDetail
            });
            model.AppraisalInfos.AddRange(infos);
            return View("CreateAppraisalInfos", model);
        }

        [HttpPost]
        [Route("[Controller]/{slug}/[Action]")]
        [ActionName("PackageRegister-Appraisal")]
        public async Task<IActionResult> ProcessAppraisalInfos(string slug, AppraisalInfosDto infosDto)
        {
            try
            {
                ViewBag.Package = _lifeInsurance.GetPackage(slug);
                var user = await GetSignedInUser();
                ModelState.Clear();
                foreach (var item in infosDto.AppraisalInfos)
                {
                    item.InsuranceCategoryId = 1;
                    item.UserId = user!.Id;
                }

                if (TryValidateModel(infosDto))
                {
                    _appraisalInfo.SetAppraisalInfos(slug, infosDto);
                    return CreateInsuredDossiers(slug);
                }

                return View("CreateAppraisalInfos", infosDto);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View("CreateAppraisalInfos", infosDto);
            }
        }

        [HttpPost]
        [Route("[Controller]/{slug}/[Action]")]
        [ActionName("PackageRegister-Dossiers-View")]
        public IActionResult CreateInsuredDossiers(string slug)
        {
            ViewBag.Package = _lifeInsurance.GetPackage(slug);
            var model = _lifeInsuredDossier.GetDossier(slug);
            return model != null ?
                View("CreateInsuredDossiers", model) :
                View("CreateInsuredDossiers");
        }

        [HttpPost]
        [Route("[Controller]/{slug}/[Action]")]
        [ActionName("PackageRegister-Dossiers")]
        public async Task<IActionResult> ProcessInsuredDossiers(
            string slug,
            AppraisalDossier dossier,
            IFormFile? pidFile = null,
            IFormFile? healthCertFile = null,
            IFormFile? otherFile = null)
        {
            try
            {
                ViewBag.Package = _lifeInsurance.GetPackage(slug);
                ModelState.Clear();
                dossier.InsuranceCategoryId = 1;
                dossier.UserId = _dbContext.applicationUser!.FirstOrDefault()!.Id;
                if (pidFile != null) dossier.Identification = pidFile.FileName;
                if (healthCertFile != null) dossier.HealthCertification = healthCertFile.FileName;
                if (otherFile != null) dossier.Other = otherFile.FileName;

                if (TryValidateModel(dossier))
                {
                    if (pidFile != null)
                        dossier.Identification = await _fileHandler.UploadFile(pidFile, "dossiers/LifeInsurance/", isTemporary: true);
                    if (healthCertFile != null)
                        dossier.HealthCertification = await _fileHandler.UploadFile(healthCertFile, "dossiers/LifeInsurance/", isTemporary: true);
                    if (otherFile != null)
                        dossier.Other = await _fileHandler.UploadFile(otherFile, "dossiers/LifeInsurance/", isTemporary: true);

                    _lifeInsuredDossier.SetDossier(slug, dossier);
                    return await CreatePackageOverview(slug);
                }

                return View("CreateInsuredDossiers", dossier);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View("CreateInsuredDossiers", dossier);
            }
        }

        [HttpGet]
        [Route("[Controller]/{slug}/[Action]")]
        [ActionName("PackageRegister-Pdf")]
        public async Task<IActionResult> PackageDemoOverView()
        {
            var packageItms = await GetDemoPackageObjects();
            if (packageItms == null) return BadRequest();
            ViewBag.DescCollapsable = false;
            return View("PackageRegisterToPdfApproved", packageItms);
        }

        private async Task<IActionResult> CreatePackageOverview(string slug)
        {
            var packageItms = await GetPackageObjects(slug); // ?? new PackageOverviewDto()
            if (packageItms == null) return BadRequest();
            return View("CreatePackageOverview", packageItms);
        }

        [HttpPost]
        [Route("[Controller]/{slug}/[Action]")]
        [ActionName("PackageRegister-Finalize")]
        public async Task<IActionResult> ProcessFinalize(string slug)
        {
            var packageItms = await GetPackageObjects(slug);
            if (packageItms == null) return BadRequest();
            var holderId = await CreatePolicyHolder(packageItms.PolicyHolder);
            if (holderId == null) return BadRequest();

            var objectId = await CreateLifeInsuredObject((int)holderId, packageItms.InsuredObject);
            if (objectId == null) return BadRequest();

            var appraisalInfos = await CreateLifeAppraisal(packageItms.User.Id, (int)objectId, packageItms.AppraisalInfos);
            if (!appraisalInfos) return BadRequest();

            var dossierId = await CreateLifeInsuredDossier(packageItms.User.Id, (int)objectId, packageItms.AppraisalDossier);
            if (dossierId == null) return BadRequest();

            return await RegisterComplete(slug, packageItms);
        }

        private async Task<string?> PackageRegisterToPdf(PackageOverviewDto packageItms)
        {
            ViewBag.DescCollapsable = false;
            var pdfContent = new ContractContent
            {
                Header = await this.RenderViewAsync("_ContractHeader", packageItms),
                Footer = await this.RenderViewAsync("_ContractFooter", packageItms),
                Body = await this.RenderViewAsync("PackageRegisterToPdf", packageItms)
            };
            string fileName = $"temp/contracts/LifeInsurance/PackageRegistration_" +
                    $"{packageItms.InsuredObject.AppraisalManifestId}_" +
                    $"{((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds()}.pdf";
            return await _pdf.HtmlToPdf(pdfContent, fileName);
        }

        private async Task Mailing(string email, string password, PackageOverviewDto packageItms)
        {
            var body = await this.RenderViewAsync("MailingRegisterComplete", packageItms);
            var attachment = await PackageRegisterToPdf(packageItms);
            var mail = new MailingData
            {
                From = email,
                To = packageItms.InsuredObject.Email,
                Password = password,
                Subject = "InsuranceService :: Package registered successfully",
                Body = body,
                FilePath = attachment
            };
            _mailing.SendMail(mail);
        }

        private async Task<IActionResult> RegisterComplete(string slug, PackageOverviewDto packageItms)
        {
            await Mailing("kientvts2204051@fpt.edu.vn", "ptge rpvw hgew mnab", packageItms);
            ViewBag.Package = _lifeInsurance.GetPackage(slug);
            ClearSetter(slug);
            return View("RegisterComplete");
        }

        private async Task<PackageOverviewDto?> GetPackageObjects(string slug)
        {
            var user = await GetSignedInUser();
            var policy = _lifeInsurance.GetPackage(slug);
            var policyHolder = _policyHolder.GetPolicyHolder(slug);
            var insuredObject = _lifeInsuredObj.GetInsuredObject(slug);
            var infos = _appraisalInfo.GetAppraisalInfos(slug);
            var dossier = _lifeInsuredDossier.GetDossier(slug);
            if (user == null || policy == null || policyHolder == null || infos == null ||
                insuredObject == null || dossier == null) return null;

            return new PackageOverviewDto
            {
                User = user,
                Package = policy,
                PolicyHolder = policyHolder,
                InsuredObject = insuredObject,
                AppraisalInfos = infos,
                AppraisalDossier = dossier
            };
        }



        private async Task<int?> CreatePolicyHolder(Policyholder holder)
        {
            await Task.FromResult(_policyHolder.Add(holder));
            return holder.Id;
        }

        private async Task<int?> CreateLifeInsuredObject(int holderId, LifeInsuredObject insuredObject)
        {
            insuredObject.PolicyholderId = holderId;
            await Task.FromResult(_lifeInsuredObj.Add(insuredObject));
            return insuredObject.Id;
        }

        private async Task<bool> CreateLifeAppraisal(string userId, int objectId, AppraisalInfosDto infos)
        {
            foreach (var info in infos.AppraisalInfos)
            {
                info.UserId = userId;
                info.LifeInsuredObjectId = objectId;
            }
            return await Task.FromResult(_appraisalInfo.AddRange(infos.AppraisalInfos));
        }

        private async Task<int?> CreateLifeInsuredDossier(string userId, int objectId, AppraisalDossier dossier)
        {
            dossier.UserId = userId;
            dossier.LifeInsuredObjectId = objectId;
            dossier.Identification = _fileHandler.MoveTempFile(dossier.Identification)!;
            dossier.HealthCertification = _fileHandler.MoveTempFile(dossier.HealthCertification)!;
            if (!string.IsNullOrEmpty(dossier.Other))
            {
                dossier.Other = _fileHandler.MoveTempFile(dossier.Other)!;
            }
            await Task.FromResult(_lifeInsuredDossier.Add(dossier));
            return dossier.Id;
        }

        private async Task<ApplicationUser?> GetSignedInUser()
        {
            string username = User.Identity?.Name ?? string.Empty;
            if (string.IsNullOrEmpty(username)) return null;
            return await _usrMgr.FindByNameAsync(username);
        }


        private void ClearSetter(string slug)
        {
            //_lifeInsurance.ClearPackage(slug);
            _lifeInsuredObj.ClearInsuredObject(slug);
            _appraisalInfo.ClearAppraisalInfos(slug);
            _lifeInsuredDossier.ClearDossier(slug);
        }

        private string GetBackUrl()
        {
            string defaultUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var referer = Request.Headers.Referer;
            var backUrl = new Uri(string.IsNullOrEmpty(referer) ? defaultUrl : referer);
            return backUrl.AbsolutePath;
        }

        private void ShowModelErrors()
        {
            var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var item in allErrors)
            {
                ModelState.AddModelError(string.Empty, item.ErrorMessage);
            }
        }

        private async Task<PackageOverviewDto?> GetDemoPackageObjects()
        {
            var user = await GetSignedInUser();
            var policy = _lifeInsurance.SetPackage(14);
            var policyHolder = _policyHolder.GetAll().FirstOrDefault();
            var insuredObject = _lifeInsuredObj.GetAll().FirstOrDefault();
            var infos = _appraisalInfo.GetAll().Take(5).ToList();
            var dossier = _lifeInsuredDossier.GetAll().FirstOrDefault();

            if (user == null || policyHolder == null || infos == null ||
                insuredObject == null || dossier == null) return null;

            return new PackageOverviewDto
            {
                User = user,
                Package = policy,
                PolicyHolder = policyHolder,
                InsuredObject = insuredObject,
                AppraisalInfos = new AppraisalInfosDto { AppraisalInfos = infos },
                AppraisalDossier = dossier
            };
        }

    }
}
