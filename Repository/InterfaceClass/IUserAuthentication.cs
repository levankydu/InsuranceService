using test0000001.Models;
using test0000001.Models.DTO;

namespace test0000001.Repository.InterfaceClass
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
