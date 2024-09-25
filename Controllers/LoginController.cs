using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using InsuranceServices.Models;
using InsuranceServices.Models.DTO;
using InsuranceServices.Repository.InterfaceClass;

namespace InsuranceServices.Controllers
{
	public class LoginController : Controller
	{
		private readonly IUserAuthentication service;
		private readonly IEmailHelper emailService;
		private readonly UserManager<ApplicationUser> userMgr;
		private readonly SignInManager<ApplicationUser> signinMgr;
		public LoginController(IUserAuthentication service, IEmailHelper emailService, UserManager<ApplicationUser> userMgr, SignInManager<ApplicationUser> signinMgr)
		{
			this.service = service;
			this.emailService = emailService;
			this.userMgr = userMgr;
			this.signinMgr = signinMgr;
		}
		public IActionResult Welcome()
		{
			return View();
		}

		public IActionResult Login()
		{
			ClaimsPrincipal claims = HttpContext.User;
			return claims.Identity!.IsAuthenticated ? RedirectToAction("index", "Home") : View();
		}

		[HttpPost]
		public async Task<IActionResult> Login(Login model)
		{

			if (!ModelState.IsValid) return View("Welcome", model);
			var result = await service.Login(model);
			if (result.StatusCode == 1) return RedirectToAction("index", "Home");
			else
			{
				ViewBag.msg = result.StatusMessage;
				return View("Welcome");
			}
		}

		[Authorize]
		public async Task<IActionResult> Logout()
		{
			await service.Logout();
			return RedirectToAction("index", "Home");
		}

		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Register(Registration model)
		{
			if (!ModelState.IsValid) return View(model);
			model.Role = "user";
			var result = await service.Register(model);
			if (result.StatusCode == 1)
			{
				string subject = "INSURANCE-REGISTRATION";
				string body = await emailService.RenderToStringAsync("../EmailTemplate/RegisterSuccess", model);
				await emailService.SendEmail(model.Email!, body, subject);
				ViewBag.msg = result.StatusMessage;
				return View("Welcome");
			}
			else
			{
				ViewBag.msg = result.StatusMessage;
				return View("Welcome");
			}
		}

		[AllowAnonymous]
		public IActionResult GoogleLogin()
		{
			string redirectUrl = Url.Action("GoogleResponse")!;
			var properties = signinMgr.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, redirectUrl);
			return Challenge(properties, GoogleDefaults.AuthenticationScheme);
		}
		[Route("Login/GoogleResponse")]
		[AllowAnonymous]
		public async Task<IActionResult> GoogleResponse(Registration model)
		{
			var info = await signinMgr.GetExternalLoginInfoAsync();
			var claims = info.Principal!.Identities
				.FirstOrDefault()!.Claims.Select(claim => new
				{
					claim.Issuer,
					claim.OriginalIssuer,
					claim.Type,
					claim.Value
				});

			var userExist = await userMgr.FindByEmailAsync(claims.First(m => m.Type == ClaimTypes.Email).Value);
			if (userExist != null)
			{
				ViewBag.count = "1";
				ViewBag.status = "Your Google Account is Registered As Member Of Our Website Already";
				ViewBag.email = userExist.Email;
				return View();
			}

			else
			{
				string Name = info.Principal.FindFirst(ClaimTypes.Name)!.Value;
				Name = ConvertVN(Name).ToString().Replace(" ", "").Replace("(", "").Replace(")", "");
				model.FirstName = Name;
				model.LastName = "viaGoogle";
				model.Email = info.Principal.FindFirst(ClaimTypes.Email)!.Value;
				model.Role = "user";
				model.Password = "1234@Abcde";

				var resultLogin = await service.Register(model);
				string subject = "INSURANCE-REGISTRATION";
				string body = await emailService.RenderToStringAsync("../EmailTemplate/GoogleRegisterSuccess", model);
				await emailService.SendEmail(model.Email, body, subject);

				if (resultLogin.StatusCode == 1)
				{
					ViewBag.count = "2";
					ViewBag.status = "Your Google Account Is Successfully Register as Member Of Our Website";
					ViewBag.email = model.Email;
					return View();
				}
				return View();
			}
		}

		private static string ConvertVN(string data)
		{
			return string.IsNullOrEmpty(data) ? string.Empty : new string(data
		.Normalize(NormalizationForm.FormD)
		.Where(ch => CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
		.ToArray());
		}

		public async Task<IActionResult> LoginNoPassword(string email)
		{
			var user = await userMgr.FindByEmailAsync(email);
			await signinMgr.SignInAsync(user, isPersistent: true);
			return RedirectToAction("index", "Home");
		}

		public async Task<IActionResult> reg()
		{
			var model = new Registration
			{
				Email = "admin@admin.com",
				FirstName = "ad",
				LastName = "min",
				Password = "1234@Abcde",
				Role = "admin"
			};
			var result = await service.Register(model);
			return Ok(result);
		}

		public async Task<IActionResult> log()
		{
			var model = new Login
			{
				Email = "user@user.com",
				Password = "1234@Abcde",
			};
			var result = await service.Login(model);
			return Ok(result);
		}
	}
}
