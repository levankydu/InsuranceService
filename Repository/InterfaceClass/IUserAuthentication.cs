using InsuranceServices.Models;
using InsuranceServices.Models.DTO;

namespace InsuranceServices.Repository.InterfaceClass
{
	public interface IUserAuthentication
	{
		Task<Status> Login(Login model);

		Task<Status> Register (Registration model);

		Task Logout();

		Task<ApplicationUser> GetCurrentUser(string username);
		Task<Status> UserIsUpdate(string username);
	}
}
