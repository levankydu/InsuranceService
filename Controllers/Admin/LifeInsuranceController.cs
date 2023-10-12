using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using test0000001.Extensions;
using test0000001.Models;
using test0000001.Models.DTO.LifeInsurance;
using test0000001.Repository.ServiceClass.LifeInsurance;

namespace test0000001.Controllers.Admin
{
    [Route("Admin/[Controller]/[Action]/{id?}")]
    [Authorize(Roles = "admin")]
    public class LifeInsuranceController : Controller
    {
        private readonly LifeInsuranceService _lifeInsurance;
        private readonly AppraisalQuesService _appraisalQues;
        private readonly PolicyHolderService _policyHolder;
        private readonly PaymentScheduleService _paymentSchedule;
        private readonly string _viewPath = "../Admin/LifeInsurance/Policy";

        public LifeInsuranceController(
            LifeInsuranceService lifeInsurance,
            AppraisalQuesService appraisalQues,
            PolicyHolderService policyHolder,
            PaymentScheduleService paymentSchedule
            )
        {
            _lifeInsurance = lifeInsurance;
            _appraisalQues = appraisalQues;
            _policyHolder = policyHolder;
            _paymentSchedule = paymentSchedule;
        }

        // GET: Admin/LifeInsurance
        public IActionResult Index()
        {
            var model = new AdminStatistic
            {
                PolicyHolderCount = _policyHolder.GetAll().Count(),
                PolicyHolderPendingCount = _policyHolder.GetByStatus("Pending").Count(),
                PolicyCount = _lifeInsurance.GetAll().Count(),
                AppraisalQuesCount = _appraisalQues.GetByCateId(1).Count(),
                DuePayments = _paymentSchedule.GetDuePayments()
            };
            return View("../Admin/LifeInsurance/Index", model);
        }

        // GET: Admin/LifeInsurance/List
        public IActionResult List()
        {
            var model = _lifeInsurance.GetAll().OrderByDescending(m => m.Id);
            return View($"{_viewPath}/Index", model);
        }

        // GET: Admin/LifeInsurance/GetDurationList
        public IActionResult GetDurationList(decimal val)
        {
            return Ok(_lifeInsurance.GetDurationsInYear(val));
        }

        // GET: Admin/LifeInsurance/Create
        public IActionResult Create()
        {
            var recentPolicies = _lifeInsurance.GetRecentCreated(6);
            ViewBag.RecentPolicies = recentPolicies;
            return View($"{_viewPath}/CreateOrEdit");
        }

        // POST: Admin/LifeInsurance/Create
        [HttpPost]
        public async Task<IActionResult> Create(Policy policy, IFormFile? photo = null)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    policy.Image = photo != null ?
                        await AddImage(photo) :
                        string.Empty;

                    policy.Slug = policy.Name?.Slugify();

                    await Task.FromResult(_lifeInsurance.Add(policy));
                    return RedirectToAction(nameof(List));
                }
                var recentPolicies = _lifeInsurance.GetRecentCreated(6);
                ViewBag.RecentPolicies = recentPolicies;
                return View($"{_viewPath}/CreateOrEdit");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View($"{_viewPath}/CreateOrEdit", policy);
            }
        }

        // GET: Admin/LifeInsurance/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                var model = await Task.FromResult(_lifeInsurance.GetById(id));
                if (model == null) return NotFound();
                ViewBag.DurationList = new SelectList(
                        _lifeInsurance.GetDurationsInYear(model.Premium),
                        "Id",
                        "Term",
                        model.DurationId);
                TempData["ImageUrl"] = model.Image;
                var recentPolicies = _lifeInsurance.GetRecentCreated(6);
                ViewBag.RecentPolicies = recentPolicies;
                return View($"{_viewPath}/CreateOrEdit", model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: Admin/LifeInsurance/Edit/{id}
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Policy policy, IFormFile? photo = null, string? photoOrigin = null)
        {
            try
            {
                //TempData["ImageUrl"] = policy.Image;
                ViewBag.DurationList = new SelectList(
                        _lifeInsurance.GetDurationsInYear(policy.Premium),
                        "Id",
                        "Term",
                        policy.DurationId);

                if (ModelState.IsValid)
                {
                    string fileName = TempData["ImageUrl"]?.ToString() ?? string.Empty;
                    if (photo != null)
                    {
                        policy.Image = await AddImage(photo);
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            RemoveImage(fileName);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fileName) && photoOrigin == null)
                        {
                            policy.Image = string.Empty;
                            RemoveImage(fileName);
                        }
                        else policy.Image = fileName;
                    }

                    policy.Slug = policy.Name?.Slugify();

                    await Task.FromResult(_lifeInsurance.Edit(policy));
                    return RedirectToAction(nameof(List));
                }
                var recentPolicies = _lifeInsurance.GetRecentCreated(6);
                ViewBag.RecentPolicies = recentPolicies;
                return View($"{_viewPath}/CreateOrEdit");
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, e.Message);
                return View($"{_viewPath}/CreateOrEdit", policy);
            }
        }

        // GET: Admin/LifeInsurance/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null) return NotFound();
                var model = await Task.FromResult(_lifeInsurance.GetById(id));
                if (model == null) return NotFound();
                TempData["ImageUrl"] = model.Image;
                return PartialView($"{_viewPath}/Delete", model);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        // POST: Admin/LifeInsurance/Delete/{id}
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            try
            {
                await Task.FromResult(_lifeInsurance.Delete(id));
                string fileName = TempData["ImageUrl"]?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(fileName))
                {
                    RemoveImage(fileName);
                }
                return RedirectToAction(nameof(List));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private async Task<string> AddImage(IFormFile file)
        {
            var mimeType = file.ContentType;
            if (!mimeType.StartsWith("image/"))
            {
                throw new Exception("Only image file is acceptable.");
            }
            string fileName = FileNameBuilder(file);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/images/LifeInsurance/", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return "images/LifeInsurance/" + fileName;
        }

        private string FileNameBuilder(IFormFile file)
        {
            DateTime currentTime = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            int rand = new Random().Next(1, 10000);
            string ext = Path.GetExtension(file.FileName);
            return unixTime.ToString() + rand.ToString() + ext;
        }

        private void RemoveImage(string fileName)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot/", fileName);
                FileInfo file = new FileInfo(filePath);
                if (file.Exists) file.Delete();
                else throw new Exception("File Not found.");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
