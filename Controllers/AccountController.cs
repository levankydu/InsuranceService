using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceServices.Controllers
{

	public class AccountController : Controller
	{
		[AllowAnonymous]
		[HttpGet]
		public IActionResult AccessDenied()
		{
			return View("AccessDenied");
		}
	}
}
