using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Xml.Linq;
using test0000001.DB;
using test0000001.Models;
using test0000001.Models.DTO.HomeInsurance;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Controllers
{
    public class HomeInsuranceController : Controller
    {
        private IWebHostEnvironment env;
        private DatabaseContext db;
        private readonly UserManager<ApplicationUser> usrMgr;
        private readonly IEmailHelper email;

        public HomeInsuranceController( IWebHostEnvironment _env, DatabaseContext _db, UserManager<ApplicationUser> _usrMgr, IEmailHelper _email)
        {
            env = _env;
            db = _db;
            usrMgr = _usrMgr;
            email = _email;
        }
        //admin page
        [Authorize(Roles = ("admin"))]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await db.Home_Insurance!.Include(h => h.Photos).ToListAsync();
			return View(model);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var model = await db.Home_Insurance!.Where(i => i.Id == id).Include(h => h.Photos).FirstOrDefaultAsync()!;
            return View(model);
        }
        public async Task<IActionResult> Introduce()
        {
            return View();
        }
       
        [Authorize(Roles = ("admin"))]
        [HttpGet]
        public IActionResult HomePolicyList()
        {
            var insurCate = db.insuranceCategory!.Where(i => i.Name == "Home Insurance").FirstOrDefault()!;
            var list = db.Policy.Include(d => d.InsuranceCategory).Include(d => d.Duration).Where(q => q.InsuranceCategoryId == insurCate.Id).ToList();
            return list.Count == 0 ? View() : View(list);
        }
        [Authorize(Roles = ("admin"))]
        [HttpGet]
        public IActionResult CreateHomePolicy()
        {
            return View();
        }
        [Authorize(Roles = ("admin"))]
        [HttpPost]
        public IActionResult CreateHomePolicy(Policy model)
        {
            var insurCate = db.insuranceCategory!.Where(i => i.Name == "Home Insurance").FirstOrDefault()!;
            if (ModelState.IsValid)
            {
                var existPolicy = db.Policy.SingleOrDefault(a =>
                    a.InsuranceCategoryId.Equals(insurCate!.Id) &&
                    a.DurationId.Equals(model.DurationId) &&
                    a.Name!.Equals(model.Name)
                );
                if (existPolicy == null)
                {
                    model.InsuranceCategoryId = insurCate!.Id;
                    model.Premium = db.Duration.Find(model.DurationId)!.PriceAmount;
                    db.Add(model);
                    db.SaveChanges();
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
        [HttpGet]
        [Authorize(Roles = "user")]
		public IActionResult Create()
        {
            //ViewData["DepId"] = new SelectList(db.Policy, "Id", "Name");
            //ViewData["DurationId"] = new SelectList(db.Duration, "Id", "Term");
            var insurCate = db.insuranceCategory!.Where(i => i.Name == "Home Insurance").FirstOrDefault()!;
            var policies = db.Policy.Include(t => t.Duration).Where(p => p.InsuranceCategoryId == insurCate!.Id).ToList();
            ViewBag.Policies = policies;
            return View();
        }

        [HttpPost]
		[Authorize(Roles = "user")]
		public async Task<IActionResult> Create(HomeInsurance_PolicyHolder home_holder, IFormFile[] photos)
        {
			Policyholder? holder = home_holder.Policyholder;
            Home_Insurance? newHome_Insurance = home_holder.HomeInsurance;
            db.Home_Insurance!.Add(newHome_Insurance);
            await db.SaveChangesAsync();
            if (photos.Count() > 0)
            {
                int count = photos.Count();
                for (int i = 0; i < count; i++)
                {
                    var item = photos[i];
                    if (item != null || item!.Length > 0)
                    {
                        var filePath = Path.Combine("wwwroot/HomeInsurance/images", item.FileName);
                        var stream = new FileStream(filePath, FileMode.Create);
                        await item.CopyToAsync(stream);
                        Photos photo = new Photos()
                        {
                            PhotoUrl = "/HomeInsurance/images/" + item.FileName,
                            Home_Insurance = db.Home_Insurance.OrderByDescending(h => h.Id).FirstOrDefault() 
                        };
                        db.Photos.Add(photo);
                        db.SaveChanges();
                    }
                }
            }
            try
            {
                holder!.Status = "Pending";
                holder.User = await GetCurrentUser();
                holder.Object =  null;
                holder.Question = null;
                holder.CarInsuredObject = GetAdminMotor();

                await db.Policyholder.AddAsync(holder);
                db.SaveChanges();

                holder.Policy = db.Policy.Include(p => p.Duration).Include(p => p.InsuranceCategory).FirstOrDefault(p => p.Id.Equals(holder.PolicyId));
                await email.SendEmail(holder.User!.Email, await email.RenderToStringAsync("../HomeInsurance/Email/Confirmation", holder), "Confirmation Notification");
                ViewBag.MsgSuccess = "Successfully, a notification email is sent to you, please check everything carefully, thank you!";
            }
            catch (Exception ex)
            {
                ViewBag.MsgError = ex.Message;
            }
            
            return View();
        }
        protected async Task<ApplicationUser> GetCurrentUser()
        {
            string username = User.Identity!.Name!.ToString();
            ApplicationUser user = await usrMgr.FindByNameAsync(username);
            return user;
        }
        protected CarInsuredObject GetAdminMotor()
        {
            CarInsuredObject model;
            ApplicationUser admin = usrMgr.GetUsersInRoleAsync("admin").Result.FirstOrDefault()!;
            if (db.CarInsuredObject!.Where(c => c.UserId == admin.Id).FirstOrDefault() != null)
            {
                model = db.CarInsuredObject!.Where(c => c.UserId == admin.Id).FirstOrDefault()!;
            }
            else
            {
                model = new CarInsuredObject()
                {
                    YearsOfManufacture = DateTime.Now,
                    Automaker = "Admin Automaker",
                    CarBand = "Admin CarBand",
                    CarType = "Admin Honda",
                    CityOfCarReg = "Admin City",
                    User = admin
                };
                db.CarInsuredObject.Add(model);
                db.SaveChanges();
            }
            return model;
        }
       

        [HttpPost]
		[Authorize(Roles = ("admin"))]
		public async Task<IActionResult> Delete(int id)
        {
			var homeInsurance = await db.Home_Insurance!.SingleOrDefaultAsync(x => x.Id.Equals(id));
			if (homeInsurance != null)
			{
				db.Home_Insurance!.Remove(homeInsurance);
				db.SaveChangesAsync();
				ViewData["msg"] = "Delete Home Insurance Success";
                return RedirectToAction("Index");

            }
			ViewData["fail"] = "Delete Home Insurance Fail";
            return RedirectToAction("Index");

        }
	}
}
