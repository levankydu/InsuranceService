using Humanizer.Localisation.TimeToClockNotation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Models;
using test0000001.Models.DTO;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Controllers
{

    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {

        private readonly UserManager<ApplicationUser> usrMgr;
        private readonly DatabaseContext db;
        private readonly IEmailHelper emailService;

        public AdminController(IUserAuthentication service, UserManager<ApplicationUser> usrMgr, DatabaseContext db, IEmailHelper emailService)
        {
            this.usrMgr = usrMgr;
            this.db = db;
            this.emailService = emailService;
        }
        [HttpGet]
        public IActionResult Display(DateTime? startDay, DateTime? endDay)
        {
            ViewBag.Payments = null;
            ViewBag.TotalAmount = null;
            bool isValid = false;
            if (startDay == null || endDay == null)
            {
                isValid = true;
            }

            if (endDay != DateTime.MinValue && startDay <= endDay)
            {
                isValid = true;
                decimal amount = 0;
                var payments = db.payment!.Include(p => p.Policyholder!.User).Where(p => p.CreatedAt >= startDay && p.CreatedAt <= endDay).ToList();
                payments.ForEach(p =>
                {
                    amount += p.Amount;
                });
                ViewBag.Payments = payments;
                ViewBag.TotalAmount = amount;
            }
            
            if (isValid == false) { 
                ViewBag.Status = "End Day must be greated than Start Day";
            }

            ViewBag.TotalPolicy = db.Policy.ToList().Count();
            ViewBag.TotalUser = usrMgr.GetUsersInRoleAsync("user").GetAwaiter().GetResult().Count();
            ViewBag.PendingHolders = db.Policyholder.Where(h => h.Status == "Pending").ToList().Count();
            ViewBag.UnreadMsg = db.GuestMessage.Where(m => m.isSent == false).ToList().Count();

            return View();
        }
        [HttpGet]
        public IActionResult ListUser()
        {

            var list = usrMgr.GetUsersInRoleAsync("user").GetAwaiter().GetResult();
            if (list != null)
            {
                return View(list);
            }
            else { return View(); }

        }
        [HttpGet]
        public IActionResult ListContactMessage()
        {
            var list = db.GuestMessage.ToList();
            if (list != null)
            {
                return View(list);
            }
            else { return View(); }
        }
        [HttpGet]
        public IActionResult SendEmail(int id)
        {
            var email = db.GuestMessage.SingleOrDefault(a => a.Id.Equals(id));
            if (email != null)
            {
                SendEmail model = new()
                {
                    Email = email.Email,
                };
                return View(model);
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SendEmail(SendEmail model)
        {
            if (ModelState.IsValid)
            {
                string userEmail = model.Email!;

                string body = await emailService.RenderToStringAsync("../EmailTemplate/ContactGuest", model);

                string subject = model.Subject!;
                await emailService.SendEmail(userEmail, body, subject);
                var message = await db.GuestMessage.SingleOrDefaultAsync(a => a.Email!.Equals(userEmail));
                message!.isSent = true;
                message.AdminAnswer = model.ContentSend;
                db.GuestMessage.Update(message);

                db.SaveChanges();
                ViewBag.status = "Email Sent successfull";
                return View();
            }
            else
            {
                return View();
            }


        }
        [HttpGet]
        public IActionResult ViewSentEmail(int id)
        {
            var email = db.GuestMessage.SingleOrDefault(a => a.Id.Equals(id));
            if (email != null)
            {

                return View(email);
            }
            return View();
        }


    }
}
