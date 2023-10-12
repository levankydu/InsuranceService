using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Claims;
using test0000001.Models;
using test0000001.Models.DTO;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Repository.ServiceClass
{

	public class UserServices : IUserAuthentication
	{
		private readonly SignInManager<ApplicationUser> signInManager;
		private readonly UserManager<ApplicationUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;
		private readonly IHttpContextAccessor contextAccessor;

		public UserServices(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor contextAccessor)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.roleManager = roleManager;
			this.contextAccessor = contextAccessor;
		}

		public Task<ApplicationUser> GetCurrentUser(string username)
		{

			var user = userManager.FindByNameAsync(username);
			return user;
		}

		public async Task<Status> Login(Login model)
		{
			var status = new Status();
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				status.StatusMessage = "Email not found";
				status.StatusCode = 0;
				return status;
			}
			// match password
			if (!await userManager.CheckPasswordAsync(user, model.Password))
			{
				status.StatusMessage = "Password wrong";
				status.StatusCode = 0;
				return status;
			}

			var signInResult = await signInManager.PasswordSignInAsync(user, model.Password, false, true);
			if (signInResult.Succeeded)
			{
				var userRoles = await userManager.GetRolesAsync(user);
				var authClaims = new List<Claim>
				{
					new Claim(ClaimTypes.Email, user.Email),
					
				};
				foreach (var userRole in userRoles)
				{
					authClaims.Add(new Claim(ClaimTypes.Role, userRole));
				}
				ClaimsIdentity claimsIdentity = new ClaimsIdentity(authClaims, CookieAuthenticationDefaults.AuthenticationScheme);
				AuthenticationProperties properties = new AuthenticationProperties()
				{
					AllowRefresh = true,
					IsPersistent = model.KeepLogin
				};

				await contextAccessor.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);


				status.StatusMessage = "Login successfully";
				status.StatusCode = 1;
				return status;
			}
			else if (signInResult.IsLockedOut)
			{
				status.StatusMessage = "User is Locked";
				status.StatusCode = 0;
				return status;
			}
			else
			{
				status.StatusMessage = "False Login";
				status.StatusCode = 0;
				return status;
			}
		}

		public async Task Logout()
		{
			await signInManager.SignOutAsync();
		}

		public async Task<Status> Register(Registration model)
		{
			var status = new Status();
			var userExist = await userManager.FindByNameAsync(model.Email);
			if (userExist != null)
			{
				status.StatusMessage = "Duplicate Email";
				status.StatusCode = 0;
				return status;
			}


			ApplicationUser user = new ApplicationUser
			{
				SecurityStamp = Guid.NewGuid().ToString(),
				FirstName = model.FirstName,
				LastName = model.LastName,
				Email = model.Email,
				UserName = model.FirstName + model.LastName,
				EmailConfirmed = true,


			};

			var result = await userManager.CreateAsync(user, model.Password);

			if (!result.Succeeded)
			{
				status.StatusMessage = "Duplicate Email or Name";
				status.StatusCode = 0;
				return status;
			}
			//set role

			if (!await roleManager.RoleExistsAsync(model.Role))
				await roleManager.CreateAsync(new IdentityRole(model.Role));

			if (await roleManager.RoleExistsAsync(model.Role))
			{
				await userManager.AddToRoleAsync(user, model.Role);
			}
			status.StatusMessage = "Success register";
			status.StatusCode = 1;
			return status;
		}

		public async Task<Status> UserIsUpdate(string username)

		{
			var status = new Status();
			var user = await userManager.FindByNameAsync(username);
			if (user.Address == null || user.DateOfBirth == DateTime.MinValue || user.Gender == null ||user.PhoneNumber.ToString()=="")
			{
				status.StatusMessage = "You have to update your profile to continue";
				status.StatusCode = 0;
				return status;

			}
			else if(contextAccessor.HttpContext!.User.IsInRole("admin"))
			{
				status.StatusMessage = "Admin can not register any policyholder";
				status.StatusCode = 0;
				return status;

			}
			else
			{
				status.StatusMessage = "OK";
				status.StatusCode = 1;
				return status;

			}
		}
	}
}
