using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayPal.Api;
using PayPalCheckoutSdk.Orders;
using System.Globalization;
using test0000001.DB;
using test0000001.Models;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Controllers
{
    [Authorize]
    public class HealthInsuranceController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly UserManager<ApplicationUser> _usrMgr;
        private readonly IEmailHelper _email;
        private readonly IUserAuthentication userIsValidate;

        public HealthInsuranceController(DatabaseContext db, UserManager<ApplicationUser> usrMgr, IEmailHelper email, IUserAuthentication userIsValidate)
        {
            _db = db;
            _usrMgr = usrMgr;
            _email = email;
            this.userIsValidate = userIsValidate;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "user")]
        public IActionResult Create(string? purpose)
        {
            if (purpose == null)
            {
                return RedirectToAction("Index");
            }
            var insurCate = GetInsurCate();
            ViewBag.Question = GetQuestions();
            ViewBag.Purpose = purpose;
            var policies = _db.Policy.Include(t => t.Duration).Where(p => p.InsuranceCategoryId == insurCate!.Id).ToList();
            ViewBag.Policies = policies;
            return View();
        }

        [HttpPost]
        [ActionName("Create")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> CreateHolder(Policyholder holder, Dictionary<string, string> objData, Dictionary<string, string> qsData, IFormFile[] idPhotos)
        {
            string username = User.Identity!.Name!.ToString();
            var userValidate = await userIsValidate.UserIsUpdate(username);
            if (userValidate.StatusCode == 0)
            {
                ViewBag.MsgError = userValidate.StatusMessage!.ToString();
                return View();
            }

            if (qsData.ContainsKey("PolicyId") && qsData.ContainsKey("objData") && qsData.ContainsKey("__RequestVerificationToken"))
            {
                qsData.Remove("PolicyId");
                qsData.Remove("objData");
                qsData.Remove("__RequestVerificationToken");
            }

            bool wasCreated = await WasCreated(objData);
            if (wasCreated)
            {
                ViewBag.MsgError = "A policy belongs to this person is currently run, please check your holders again!";
            }
            else
            {
                if (idPhotos.Count() > 0)
                {
                    int count = idPhotos.Count();
                    for (int i = 0; i < count; i++)
                    {
                        var item = idPhotos[i];
                        if (item != null || item!.Length > 0)
                        {
                            var filePath = Path.Combine("wwwroot/HealthInsurance/IdImages", item.FileName);
                            var stream = new FileStream(filePath, FileMode.Create);
                            await item.CopyToAsync(stream);
                            objData["id_photo_" + (i == 0 ? "front" : "back")] = "HealthInsurance/IdImages/" + item.FileName;
                        }
                    }
                }

                try
                {
                    holder!.Status = "Pending";
                    holder.User = await GetCurrentUser();
                    holder.Object = objData.Count() > 0 ? await CreateObject(objData) : null;
                    holder.Question = qsData.Count() > 0 ? await CreateObject(qsData) : null;
                    holder.CarInsuredObject = GetAdminMotor();

                    await _db.Policyholder.AddAsync(holder);
                    _db.SaveChanges();

                    holder.Policy = _db.Policy.Include(p => p.Duration).Include(p => p.InsuranceCategory).FirstOrDefault(p => p.Id.Equals(holder.PolicyId));
                    await _email.SendEmail(holder.User!.Email, await _email.RenderToStringAsync("../HealthInsurance/Email/Confirmation", holder), "Confirmation Notification");
                    ViewBag.MsgSuccess = "Successfully, a notification email is sent to you, please check everything carefully, thank you!";
                }
                catch (Exception ex)
                {
                    ViewBag.MsgError = ex.Message;
                }
            }
            //return BadRequest(holder);
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult GetDurationList(decimal minPrice, decimal maxPrice)
        {
            if (minPrice > maxPrice)
            {
                return Json(new
                {
                    error = "Min price is greated than max price"
                }); ;
            }
            return Json(new
            {
                list = FilterDurationByPrice(minPrice, maxPrice)
            });
        }

        [Authorize(Roles = ("admin"))]
        [HttpGet]
        public IActionResult QuestionIndex()
        {
            List<Question> questions = GetQuestions();
            return View(questions);
        }

        [Authorize(Roles = ("admin"))]
        [HttpGet]
        public IActionResult QuestionCreate()
        {
            return View();
        }

        [Authorize(Roles = ("admin"))]
        [HttpPost]
        public IActionResult QuestionCreate(Question question)
        {
            var insurCate = GetInsurCate();
            question.InsuranceCategoryId = insurCate.Id;
            _db.Questions.Add(question);
            _db.SaveChanges();
            return RedirectToAction("QuestionIndex");
        }

        [Authorize(Roles = ("admin"))]
        [HttpGet]
        public IActionResult CreateHealthPolicy()
        {
            return View();
        }
        [Authorize(Roles = ("admin"))]
        [HttpPost]
        public IActionResult CreateHealthPolicy(Policy model)
        {
            var insurCate = GetInsurCate();
            if (ModelState.IsValid)
            {
                var existPolicy = _db.Policy.SingleOrDefault(a =>
                    a.InsuranceCategoryId.Equals(insurCate!.Id) &&
                    a.DurationId.Equals(model.DurationId) &&
                    a.Name!.Equals(model.Name)
                );
                if (existPolicy == null)
                {
                    model.InsuranceCategoryId = insurCate!.Id;
                    model.Premium = _db.Duration.Find(model.DurationId)!.PriceAmount;
                    _db.Add(model);
                    _db.SaveChanges();
                    ViewBag.MsgSuccess = "Successfull Created ";
                }
                else
                {
                    ViewBag.MsgError = "This policy holder is existed, please check again!";
                }
            }
            else
            {
                ViewBag.MsgError = "Failed Created";
            }
            return View();
        }

        [Authorize(Roles = ("admin"))]
        [HttpGet]
        public IActionResult HealthPolicyList()
        {
            var healthCate = GetInsurCate();
            var list = _db.Policy.Include(d => d.InsuranceCategory).Include(d => d.Duration).Where(q => q.InsuranceCategoryId == healthCate.Id).ToList();
            return list.Count == 0 ? View() : View(list);
        }

        protected async Task<string> CreateObject(Dictionary<string, string> data)
        {
            dynamic jsonObject = new JObject();
            if (data.ContainsKey("purpose") && data["purpose"] == "self")
            {
                ApplicationUser user = await GetCurrentUser();
                jsonObject["first_name"] = user.FirstName;
                jsonObject["last_name"] = user.LastName;
                jsonObject["dob"] = user.DateOfBirth;
                jsonObject["email"] = user.Email;
                jsonObject["phone"] = user.PhoneNumber;
                jsonObject["gender"] = user.Gender;
            }
            if (data != null)
            {
                foreach (var item in data)
                {
                    jsonObject[item.Key] = item.Value;
                }
            }
            string jsonResult = jsonObject.ToString();
            return jsonResult;
        }

        protected async Task<bool> WasCreated(Dictionary<string, string> data)
        {
            ApplicationUser user = await GetCurrentUser();
            List<Policyholder> holders = GetHolders(user.Id);
            foreach (var holder in holders)
            {
                if (holder.Object != null)
                {
                    dynamic? obj = JsonConvert.DeserializeObject<dynamic>(holder.Object!);
                    if (obj != null)
                    {
                        bool isSamePurpose = obj.purpose == data["purpose"];
                        bool isSameIdNumber = false;

                        if (data["purpose"] != "self")
                        {
                            isSameIdNumber = obj.id_number == data["id_number"];
                        }

                        DateTime currentDate = DateTime.Today;
                        bool isEndDateBeforeToday = holder.EndDay < currentDate;
                        bool isPending = holder.Status == "Pending";

                        if (isSamePurpose && data["purpose"] == "self") return true;

                        if (isSamePurpose && isSameIdNumber && isEndDateBeforeToday && isPending) return true;

                        if (isSamePurpose && isSameIdNumber && isEndDateBeforeToday) return false;
                    }
                }


            }
            return false;
        }

        protected async Task<ApplicationUser> GetCurrentUser()
        {
            string username = User.Identity!.Name!.ToString();
            ApplicationUser user = await _usrMgr.FindByNameAsync(username);
            return user;
        }

        protected InsuranceCategory GetInsurCate()
        {
            return _db.insuranceCategory!.Where(i => i.Name == "Health Insurance").FirstOrDefault()!;
        }

        protected List<Policyholder> GetHolders(string userId)
        {
            var healthCate = GetInsurCate();
            List<Policyholder> holders = _db.Policyholder
                .Include(h => h.Policy!.InsuranceCategory)
                .Where(h => h.UserId == userId && h.Policy!.InsuranceCategoryId == healthCate!.Id)
                .OrderByDescending(h => h.Id)
                .ToList();
            return holders;
        }

        protected List<Question> GetQuestions()
        {
            var healthCate = GetInsurCate();
            List<Question>? list = _db.Questions.Where(q => q.InsuranceCategoryId == healthCate.Id).ToList();
			return list;
        }

        protected List<Duration> FilterDurationByPrice(decimal minPrice, decimal maxPrice)
        {
            return _db.Duration
                    .Where(m => m.PriceAmount <= maxPrice && m.PriceAmount >= minPrice)
                    .ToList();
        }

        protected CarInsuredObject GetAdminMotor()
        {
            CarInsuredObject model;
            ApplicationUser admin = _usrMgr.GetUsersInRoleAsync("admin").Result.FirstOrDefault()!;
            if (_db.CarInsuredObject!.Where(c => c.UserId == admin.Id).FirstOrDefault() != null)
            {
                model = _db.CarInsuredObject!.Where(c => c.UserId == admin.Id).FirstOrDefault()!;
            }
            else {
                model = new CarInsuredObject()
                {
                    YearsOfManufacture = DateTime.Now,
                    Automaker = "Admin Automaker",
                    CarBand = "Admin CarBand",
                    CarType = "Admin Honda",
                    CityOfCarReg = "Admin City",
                    User = admin
                };
                _db.CarInsuredObject.Add(model);
                _db.SaveChanges();
            }
            return model;
        }
    }
}
