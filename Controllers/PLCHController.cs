using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Cryptography;
using test0000001.DB;
using test0000001.Models;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Controllers
{
    [Authorize(Roles = "admin")]
    public class PLCHController : Controller
    {
        private readonly DatabaseContext _db;
        private readonly IEmailHelper _email;
        public PLCHController(DatabaseContext db, IEmailHelper email)
        {
            _db = db;
            _email = email;
        }
        public IActionResult Index()
        {
            var holders = _db.Policyholder
                .Include(p => p.User)
                .Include(p => p.Policy!.InsuranceCategory)
                .Where(p => p.Policy!.InsuranceCategory!.Id != 1)
                .Include(p => p.Payments)
                .OrderByDescending(h => h.Status == "Pending")
                .ToList();
            return View(holders);
        }

        public IActionResult Details(int id) {
            var holder = GetHolderById(id);
            return View(holder);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrReject(int id, string type) {
            var holder = GetHolderById(id);
            if (type == "Reject" && (holder.Status != "Waiting For Payment" && holder.Status != "Pending")) {
                ViewBag.MsgError = "Denied, this holder is currently activated!";
                return View("Details", holder);
            }

            holder.Status = type == "Approve" ?  "Waiting For Payment" : "Rejected";
            holder.ApprovedDay = DateTime.Now;
            _db.SaveChanges();
            
            try
            {
                string viewPath = type == "Approve" ? "../EmailTemplate/Approval" : "../EmailTemplate/Rejection";
                string subject = type == "Approve" ? "Approval Notification" : "Rejection Notification";
                string view = await _email.RenderToStringAsync(viewPath, holder);
                await _email.SendEmail(holder.User!.Email, view, subject);
            }
            catch (Exception)
            {
                ViewBag.MsgError = "Something went wrong while trying to send mail, please check again!";
                return View("Details", holder);
            }

            ViewBag.MsgSuccess = "Holder status has been changed successfully!";
            return View("Details", holder);
        }

        protected Policyholder GetHolderById(int id) {
			var query = _db.Policyholder
				.Include(h => h.Policy!.Duration)
				.Include(h => h.Policy!.InsuranceCategory)
				.Include(h => h.User)
				.Include(h => h.Payments)
				.Where(h => h.Id == id);

			bool hasInsuranceCategoryId4 = query.Any(h => h.Policy!.InsuranceCategoryId == 4);

			if (hasInsuranceCategoryId4)
			{
				query = query.Include(h => h.CarInsuredObject);
			}

			Policyholder? result = query.FirstOrDefault();
			return result;
		}
    }
}
