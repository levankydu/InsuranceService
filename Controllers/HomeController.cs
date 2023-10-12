using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics;
using System.Text;
using test0000001.DB;
using test0000001.Models;
using test0000001.Models.DTO;
using test0000001.Repository.InterfaceClass;
using PayPal.Api;
using InsurancePayment = test0000001.Models.Payment;
using Payment = PayPal.Api.Payment;
using Payer = PayPal.Api.Payer;
using Item = PayPal.Api.Item;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace test0000001.Controllers
{
	public class HomeController : Controller
	{
		private readonly IUserAuthentication service;
		private readonly UserManager<ApplicationUser> usrMgr;
		private readonly ICountry country;
		private readonly DatabaseContext _db;
		private readonly IEmailHelper emailService;
		private Payment payment;

		public HomeController(IUserAuthentication service, UserManager<ApplicationUser> usrMgr, ICountry country, IEmailHelper emailService, DatabaseContext db)
		{
			this.service = service;
			this.usrMgr = usrMgr;
			this.country = country;
			this.emailService = emailService;
			_db = db;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Profile(string id)
		{
			string username = User.Identity!.Name!.ToString();
			var user = await usrMgr.FindByNameAsync(username);
			if (user != null)
			{
				return View(user);
			}
			else
			{
				return View();
			}
		}

		[HttpGet]
		[Authorize(Roles = ("user"))]
		public async Task<IActionResult> PolicyHolder()
		{
			string username = User.Identity!.Name!.ToString();
			if (username != "admin")
			{
				ApplicationUser user = await usrMgr.FindByNameAsync(username);
				var holders = GetHolders(user.Id);
				if (holders != null)
				{
					return View(holders);
				}
				else
				{
					return View();
				}
			}
			ViewBag.status = "Admin can not get policyholder";
			return View(new List<Policyholder> { });
		}

		[HttpGet]
		[Authorize(Roles = ("user"))]
		public IActionResult HolderDetails(int id)
		{
			var holder = GetHolder(id);
			return View(holder);
		}

		[HttpGet]
		public IActionResult getDistrict(string code)
		{
			var districts = country.getDistricts(code);

			return Json(new
			{
				district = districts,
			});
		}

		[HttpGet]
		public IActionResult getWard(string district)
		{
			var wards = country.getWards(district);

			return Json(new
			{
				ward = wards,
			});
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> EditProfile()
		{
			var cities = country.getCity();

			ViewBag.city = new SelectList(cities, "code", "name");


			string username = User.Identity!.Name!.ToString();
			ApplicationUser user = await usrMgr.FindByNameAsync(username);
			if (user != null)
			{

				return View(user);
			}
			else
			{
				return View();
			}
		}

		[HttpPost]
		public async Task<IActionResult> EditProfile(ApplicationUser editUser)
		{
			var cities = country.getCity();

			ViewBag.city = new SelectList(cities, "code", "name");
			string username = User.Identity!.Name!.ToString();
			ApplicationUser user = await usrMgr.FindByNameAsync(username);
			//update user
			user.FirstName = editUser.FirstName;
			user.LastName = editUser.LastName;
			user.DateOfBirth = editUser.DateOfBirth;
			user.Gender = editUser.Gender;
			user.PhoneNumber = editUser.PhoneNumber;
			user.Address = editUser.Address;


			await usrMgr.UpdateAsync(user);
			ViewBag.status = "Done update information";
			return View(user);

		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> ChangePassword()
		{
			string username = User.Identity!.Name!.ToString();
			ApplicationUser user = await usrMgr.FindByNameAsync(username);
			ViewBag.user = user.FirstName + user.LastName;
			return View();
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> ChangePassword(ChangePassword model)
		{
			string username = User.Identity!.Name!.ToString();
			ApplicationUser user = await usrMgr.FindByNameAsync(username);
			ViewBag.user = user.FirstName + user.LastName;
			//check old password match
			if (!await usrMgr.CheckPasswordAsync(user, model.OldPassword))
			{
				ViewBag.status = "Old Password not match";
				return View(model);
			}
			//old password is match , go update new password

			var token = await usrMgr.GeneratePasswordResetTokenAsync(user);

			var result = await usrMgr.ResetPasswordAsync(user, token, model.NewPassword);
			if (!result.Succeeded)
			{
				ViewBag.status = "Error Changing Password";
				return View(model);
			}
			else
			{
				ViewBag.status = "Update password successfully";
				return View(model);
			}
		}

		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> ForgotPassword(string email)
		{
			var user = await usrMgr.FindByEmailAsync(email);

			if (user == null)
			{
				ViewBag.status = "No user match";
				return View();
			}

			var token = await usrMgr.GeneratePasswordResetTokenAsync(user);

			string link = Url.Action("ResetPassword", "Home", new { token, email = user.Email }, Request.Scheme)!;
			user.ProfilePicture = link.ToString();
			var body = await emailService.RenderToStringAsync("../EmailTemplate/ForgotPassword", user);
			string subject = "INSURANCE-PASSWORD RESET";

			var emailResponse = await emailService.SendEmail(user.Email, body!, subject);

			if (emailResponse.StatusCode == 1)
			{
				ViewBag.status = emailResponse.StatusMessage;
				return View();
			}
			return View();
		}

		[AllowAnonymous]
		[HttpGet]
		public IActionResult ResetPassword(string token, string email)
		{
			var model = new ResetPassword { Token = token, Email = email };
			return View(model);
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
		{
			var user = await usrMgr.FindByEmailAsync(resetPassword.Email);
			if (user == null)
			{
				ViewBag.status = "Email not match";
				return View();
			}

			var resetPassResult = await usrMgr.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
			if (!resetPassResult.Succeeded)
			{
				ViewBag.status = "Fail update";
				return View();
			}
			ViewBag.status = "Success update your password via email authentication";
			return View();
		}

		[AllowAnonymous]
		[HttpGet]
		public IActionResult OurService()
		{
			return View();
		}
		[AllowAnonymous]
		[HttpGet]
		public IActionResult AboutUs()
		{
			return View();
		}
		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> ContactUsAsync()
		{
			GuestMessage guest = new GuestMessage();
			if (User.Identity!.IsAuthenticated)
			{
				string username = User.Identity.Name!.ToString();
				ApplicationUser user = await usrMgr.FindByNameAsync(username);
				if (user.UserName == "admin")
				{
					return RedirectToAction("AccessDenied", "Account");
				}
				else
				{
					guest.PhoneNumber = user.PhoneNumber;
					guest.Name = user.FirstName + "-" + user.LastName;
					guest.Email = user.Email;
					return View(guest);
				}
			}
			else
			{
				return View();
			}
		}
		[AllowAnonymous]
		[HttpPost]
		public IActionResult ContactUs(GuestMessage model)
		{
			_db.GuestMessage.Add(model);
			_db.SaveChanges();
			ViewBag.status = "Your Request Has Been Sent Successfully";
			return View("ContactUs");
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> ChangeProfilePicture(IFormFile photo)
		{
			string username = User.Identity!.Name!.ToString();
			var user = await usrMgr.FindByNameAsync(username);

			if (photo != null || photo!.Length > 0)
			{
				var filePath = Path.Combine("wwwroot/ProfilePicture", photo.FileName);
				var stream = new FileStream(filePath, FileMode.Create);
				await photo.CopyToAsync(stream);
				user.ProfilePicture = "ProfilePicture/" + photo.FileName;

				await usrMgr.UpdateAsync(user);


			}
			return RedirectToAction("Profile", "Home");
		}

		// start paypal
		[Authorize(Roles = ("user"))]
		public IActionResult PaymentWithPaypal(string? Cancel = null, int holderId = 0)
		{
			var holder = holderId != 0 ? GetHolder(holderId) : null;
			APIContext apiContext = PaypalConfiguration.GetAPIContext();
			try
			{
				string payerId = Request.Query["PayerID"];
				if (string.IsNullOrEmpty(payerId) && (holder!.Id != 0 || holder != null))
				{
					string baseURI = Request.Scheme + "://" + Request.Host + "/Home/PaymentWithPayPal?";
					var guid = Convert.ToString((new Random()).Next(100000));
					string redirectUrl = baseURI + "guid=" + guid;
					var createdPayment = this.CreatePayment(apiContext, redirectUrl, holder);
					var links = createdPayment.links.GetEnumerator();
					string? paypalRedirectUrl = null;
					while (links.MoveNext())
					{
						Links lnk = links.Current;
						if (lnk.rel.ToLower().Trim().Equals("approval_url"))
						{
							paypalRedirectUrl = lnk.href;
						}
					}
					HttpContext.Session.SetString(guid, createdPayment.id);
					HttpContext.Session.SetString("holderId", holderId.ToString());
					return Redirect(paypalRedirectUrl!);
				}
				else
				{
					var guid = Request.Query["guid"];
					string paymentId = HttpContext.Session.GetString(guid)!;
					var executedPayment = ExecutePayment(apiContext, payerId, paymentId);
					var condition = executedPayment.state.ToLower() != "approved";
					if (condition)
					{
						return View("FailureView");
					}
				}
			}
			catch (Exception)
			{
				return View("FailureView");
			}

			CreateInsurPayment(Int32.Parse(HttpContext.Session.GetString("holderId")!), "Paypal");
			HttpContext.Session.Remove("holderId");

			return View("SuccessView");
		}
		private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
		{
			var paymentExecution = new PaymentExecution()
			{
				payer_id = payerId
			};
			this.payment = new Payment()
			{
				id = paymentId
			};
			return this.payment.Execute(apiContext, paymentExecution);
		}
		private Payment CreatePayment(APIContext apiContext, string redirectUrl, Policyholder holder)
		{
			//create itemlist and add item objects to it  
			var itemList = new ItemList()
			{
				items = new List<Item>()
			};
			//Adding Item Details like name, currency, price etc  
			int price = Convert.ToInt32(holder.EachPeriodPrice());
			//var rootPrice = 5;

			itemList.items.Add(new Item()
			{
				name = holder.Policy!.Name!.ToString(),
				currency = "USD",
				price = price.ToString(),
				quantity = "1",
				sku = "ISR-#" + holder.PolicyId.ToString() + holder.Policy.DurationId,
			});
			var payer = new Payer()
			{
				payment_method = "paypal"
			};
			// Configure Redirect Urls here with RedirectUrls object  
			var redirUrls = new RedirectUrls()
			{
				cancel_url = redirectUrl + "&Cancel=true",
				return_url = redirectUrl
			};
			// Adding Tax, shipping and Subtotal details  
			var tax = Convert.ToDouble(price) * 0.02;
			var shipping = 0;
			//var subTotal = rootPrice * itemList.items.Count();
			var subTotal = Convert.ToDouble(price) * 1;

			var details = new Details()
			{
				tax = tax.ToString(),
				shipping = shipping.ToString(),
				subtotal = subTotal.ToString()
			};
			//Final amount with details  
			var amount = new Amount()
			{
				currency = "USD",
				total = (tax + shipping + subTotal).ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
				details = details
			};
			var transactionList = new List<Transaction>();
			// Adding description about the transaction  
			var paypalOrderId = DateTime.Now.Ticks;
			transactionList.Add(new Transaction()
			{
				description = $"Invoice #{paypalOrderId}",
				invoice_number = paypalOrderId.ToString(), //Generate an Invoice No    
				amount = amount,
				item_list = itemList
			});
			this.payment = new Payment()
			{
				intent = "sale",
				payer = payer,
				transactions = transactionList,
				redirect_urls = redirUrls
			};

			// Create a payment using a APIContext  
			return this.payment.Create(apiContext);
		}
		//end paypal

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		protected List<Policyholder> GetHolders(string userId)
		{
			List<Policyholder> holders = _db.Policyholder
				.Include(h => h.Policy!.InsuranceCategory)
				.Where(h => h.Policy!.InsuranceCategory!.Id != 1)
				.Include(h => h.Payments)
				.Where(h => h.UserId == userId)
				.OrderByDescending(h => h.Id)
				.ToList();
			return holders;
		}
		protected Policyholder GetHolder(int id)
		{
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
		protected async void CreateInsurPayment(int holderId, string method)
		{
			Policyholder holder = GetHolder(holderId);
			decimal price = holder.EachPeriodPrice();
			var subTotal = price * 1;

			holder.Status = "Activated";
			if (holder.StartDay == DateTime.MinValue || holder.EndDay == DateTime.MinValue)
			{
				holder.StartDay = DateTime.Now;
				holder.EndDay = DateTime.Now.AddMonths(holder.Policy!.Duration!.Term);
			}
			InsurancePayment? lastPaid = holder.LastestPayment();

			var payment = new InsurancePayment
			{
				CreatedAt = DateTime.Now,
				Policyholder = holder,
				Method = method,
				Amount = subTotal,
				PaymentPeriod = lastPaid != null ? lastPaid.PaymentPeriod + 1 : 1,
			};

			_db.payment!.Add(payment);
			_db.SaveChanges();

			string view = await emailService.RenderToStringAsync("../EmailTemplate/Payment", holder);
			await emailService.SendEmail(holder.User!.Email, view, "Payment Sent");
		}
	}
}