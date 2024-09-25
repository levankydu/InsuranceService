using Microsoft.AspNetCore.Mvc;
using InsuranceServices.DB;
using InsuranceServices.Models.LifeInsurance;
using InsuranceServices.Models.DTO.LifeInsurance;
using InsuranceServices.Repository.ServiceClass.LifeInsurance;
using Microsoft.AspNetCore.Authorization;

namespace InsuranceServices.Controllers.Admin
{
    [Route("Admin/[Controller]/[Action]/{id?}")]
    [Authorize]
    public class AppraisalInfoController : Controller
    {
        private readonly AppraisalInfoService _appraisalInfo;
        private readonly AppraisalQuesService _appraisalQues;
        private readonly DatabaseContext _dbContext;
        private readonly string _viewPath = "../Admin/AppraisalInfo";

        public AppraisalInfoController(AppraisalInfoService appraisalInfo, AppraisalQuesService appraisalQues, DatabaseContext dbContext)
        {
            _appraisalInfo = appraisalInfo;
            _appraisalQues = appraisalQues;
            _dbContext = dbContext;
        }

        // GET: Admin/AppraisalInfo
        public IActionResult Index()
        {
            var model = _appraisalInfo.GetAll();
            return View($"{_viewPath}/Index", model);
        }

        // GET: Admin/AppraisalInfo/Create
        public IActionResult Create()
        {

            var ques = _appraisalQues.GetByCateId(1);
            var model = new AppraisalInfosDto();
            var infos = ques.Select(q => new AppraisalInfo
            {
                Description = q.Description,
                DescriptionType = q.DescriptionType,
                DescriptionDetail = q.DescriptionDetail
            });
            model.AppraisalInfos.AddRange(infos);
            return View($"{_viewPath}/CreateOrEdit", model);
        }

        // POST: Admin/AppraisalInfo/Create
        [HttpPost]
        public IActionResult Create(AppraisalInfosDto model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var manifestId = Guid.NewGuid();
                    foreach (var item in model.AppraisalInfos)
                    {
                        item.InsuranceCategoryId = 1;
                        item.UserId = _dbContext.applicationUser!.FirstOrDefault()!.Id;
                    }
                    return Ok(model);
                    //return RedirectToAction(nameof(Index));
                }
                return View($"{_viewPath}/CreateOrEdit", model);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View($"{_viewPath}/CreateOrEdit", model);
            }
        }
    }
}


