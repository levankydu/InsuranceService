using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace test0000001.Controllers
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
