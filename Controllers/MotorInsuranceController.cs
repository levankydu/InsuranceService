using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using System.Data;
using test0000001.DB;
using test0000001.Models;
using test0000001.Repository.InterfaceClass;
using test0000001.Repository.ServiceClass;

namespace test0000001.Controllers
{
    public class MotorInsuranceController : Controller
    {
        private IInsuranceCategory insurCateSer;
        private IPolicy policyService;
        private IMotorInsurance motorInsSer;

        private IDuration durationService;

        private DatabaseContext dbContext;

        private readonly UserManager<ApplicationUser> userManager;
        public MotorInsuranceController
            (IDuration _durationService,
            IPolicy _policyService,
            DatabaseContext _dbContext,
            IMotorInsurance _motorInsSer,
            IInsuranceCategory _insurCateSer,
            UserManager<ApplicationUser> _userManager)
        {
            insurCateSer = _insurCateSer;
            durationService = _durationService;
            policyService = _policyService;
            dbContext = _dbContext;
            motorInsSer = _motorInsSer;
            userManager = _userManager;
        }



        // GET: Home/MotorInsurance
        public async Task<IActionResult> Index()
        {
            //Lấy tất cả car theo user id
            var user = await GetUser();
            var car = await dbContext.CarInsuredObject!
            .Where(p => p.UserId == user.Id) // So sánh với user.Id mà không cần ép kiểu
                .ToListAsync();

            return View(car);
        }
        [HttpGet]
        public IActionResult IntroduceMotorInsurance()
        {
            return View();
        }






        [Authorize(Roles = "user")]
        [HttpGet]
        public IActionResult TakeInforCar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TakeInforCar(CarInsuredObject newMotor, IFormFile[] idPhotos)
        {
            if (newMotor.YearsOfManufacture == DateTime.MinValue)
            {
                ModelState.AddModelError("YearsOfManufacture", "Years of Manufacture is required.");
            }

            if (string.IsNullOrEmpty(newMotor.Automaker))
            {
                ModelState.AddModelError("Automaker", "Automaker is required.");
            }

            if (string.IsNullOrEmpty(newMotor.CarBand))
            {
                ModelState.AddModelError("CarBand", "Car Band is required.");
            }

            if (string.IsNullOrEmpty(newMotor.CarType))
            {
                ModelState.AddModelError("CarType", "Car Type is required.");
            }

            if (string.IsNullOrEmpty(newMotor.EngineNumber))
            {
                ModelState.AddModelError("EngineNumber", "Engine Number is required.");
            }
            if (string.IsNullOrEmpty(newMotor.FrameNumber))
            {
                ModelState.AddModelError("FrameNumber", "Frame Number is required.");
            }
            if (string.IsNullOrEmpty(newMotor.LicensePlateNumber))
            {
                ModelState.AddModelError("LicensePlateNumber", "License Plate Number is required.");
            }
            if (string.IsNullOrEmpty(newMotor.EngineDisplacement))
            {
                ModelState.AddModelError("EngineDisplacement", "Engine Displacement is required.");
            }
            if (string.IsNullOrEmpty(newMotor.Color))
            {
                ModelState.AddModelError("Color", "Color is required.");
            }

            if (string.IsNullOrEmpty(newMotor.CityOfCarReg))
            {
                ModelState.AddModelError("CityOfCarReg", "City of Car Registration is required.");
            }

            if (!ModelState.IsValid)
            {
                return View(newMotor);
            }

            // Lưu ảnh vào thư mục wwwroot/MotorInsurance/IdImages/
            if (idPhotos != null && idPhotos.Length > 0)
            {
                for (int i = 0; i < idPhotos.Length; i++)
                {
                    var item = idPhotos[i];
                    if (item != null && item.Length > 0)
                    {
                        var filePath = Path.Combine("wwwroot/MotorInsurance/IdImages/", item.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.CopyToAsync(stream);
                        }

                        if (i == 0)
                        {
                            newMotor.FrontImg = item.FileName;
                        }
                        else
                        {
                            newMotor.BackImg = item.FileName;
                        }
                    }
                }
            }

            var user = await GetUser();
            newMotor.UserId = user.Id;

            // Thêm mới dữ liệu vào cơ sở dữ liệu
            await dbContext.CarInsuredObject!.AddAsync(newMotor);
            await dbContext.SaveChangesAsync();

            return RedirectToAction("CreateHolder", "MotorInsurance");
        }

        [Authorize(Roles = "user")]
        [HttpGet]
        public async Task<IActionResult> CreateHolder()
        {
            try
            {
                var insurCate = await insurCateSer.GetCategoryById(4);
                var policies = await dbContext.Policy.Include(t => t.Duration).Where(p => p.InsuranceCategoryId == insurCate!.Id).ToListAsync();
                ViewBag.Policies = policies;

                var user = await GetUser();
                var carInsuredObjects = await dbContext.CarInsuredObject!.Where(p => p.UserId == user.Id).ToListAsync();
                ViewBag.CarInsuredObjects = carInsuredObjects;
                ViewBag.UserId = user.Id;
                return View();
            }
            catch (Exception ex)
            {
                // In inner exception để xem lỗi cụ thể
                ViewBag.MsgError = ex.InnerException?.Message;
                return RedirectToAction("Index", "MotorInsurance");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateHolder(Policyholder policyHolder)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Xác minh rằng dữ liệu là hợp lệ trước khi lưu vào cơ sở dữ liệu
                    await dbContext.Policyholder.AddAsync(policyHolder);
                    await dbContext.SaveChangesAsync();

                    // Lưu dữ liệu thành công, chuyển hướng hoặc thực hiện các hành động khác ở đây
                    return RedirectToAction("PolicyHolder", "Home");
                }
                else
                {
                    // Trả về View với ModelState không hợp lệ để hiển thị thông báo lỗi
                    var insurCate = await insurCateSer.GetCategoryById(4);
                    var policies = await dbContext.Policy.Include(t => t.Duration).Where(p => p.InsuranceCategoryId == insurCate!.Id).ToListAsync();
                    ViewBag.Policies = policies;

                    var user = await GetUser();
                    var carInsuredObjects = await dbContext.CarInsuredObject!.Where(p => p.UserId == user.Id).ToListAsync();
                    ViewBag.CarInsuredObjects = carInsuredObjects;

                    ModelState.AddModelError(string.Empty, "There are errors in the form. Please correct them.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                // In inner exception để xem lỗi cụ thể
                ViewBag.MsgError = ex.InnerException?.Message;
                return View();
            }
        }

        protected async Task<ApplicationUser> GetUser()
        {
            if (User.Identity != null && User.Identity.Name != null)
            {
                string username = User.Identity.Name;
                ApplicationUser user = await userManager.FindByNameAsync(username);
                return user;
            }
            else
            {
                // Xử lý khi User.Identity hoặc User.Identity.Name là null
                // Có thể trả về null hoặc thông báo lỗi tùy thuộc vào logic của bạn.
                return null;
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> MotorPolicyList()
        {
            var insurCate = await insurCateSer.GetCategoryById(4);
            var policies = await dbContext.Policy.Include(t => t.Duration).Where(p => p.InsuranceCategoryId == insurCate!.Id).ToListAsync();
            return policies.Count == 0 ? View() : View(policies);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> CreateMotorPolicy()
        {

            var insuranceCategory = await insurCateSer.GetAllCategory();
            ViewBag.CategoryId = new SelectList(insuranceCategory, "Id", "Name");
            var duration = await durationService.GetAllDuration();
            ViewBag.DurationId = new SelectList(duration, "Id", "Term");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateMotorPolicy(Policy newPolicy)
        {
            await policyService.AddPolicy(newPolicy);
            return RedirectToAction("MotorPolicyList", "MotorInsurance");
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> EditMotorPolicy(int id)
        {
            var insuranceCategory = await insurCateSer.GetAllCategory();
            //ViewBag.TeamId = new SelectList(teams.Select(x => x.Id));
            ViewBag.CategoryId = new SelectList(insuranceCategory, "Id", "Name");
            var duration = await durationService.GetAllDuration();
            ViewBag.DurationId = new SelectList(duration, "Id", "Term");

            var model = await policyService.GetPolicyById(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditMotorPolicy(Policy editPolicy)
        {
            await policyService.EditPolicy(editPolicy);
            TempData["msg"] = "Congratulation !!! Edit Success";
            return RedirectToAction("MotorPolicyList", "MotorInsurance");
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteMotorPolicy(int id)
        {
            try
            {
                var model = await policyService.DeletePolicy(id);
                TempData["msg"] = "Delete Policy Success";
                return RedirectToAction("MotorPolicyList", "MotorInsurance");

            }
            catch (Exception)
            {
                ViewBag.errormsg = "Delete Policy Fail";
                return RedirectToAction("MotorPolicyList", "MotorInsurance");
            }
        }

        //CHƯA CÓ VIEW
        public async Task<IActionResult> MotorObjectList()
        {
            //Lấy tất cả car theo user id
            var user = await GetUser();
            var MotorObject = await dbContext.CarInsuredObject!
            .Where(p => p.UserId == user.Id) // So sánh với user.Id mà không cần ép kiểu
                .ToListAsync();

            return View(MotorObject);
        }

    }
}
