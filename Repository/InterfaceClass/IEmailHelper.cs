using Microsoft.AspNetCore.Mvc;
using test0000001.Models.DTO;

namespace test0000001.Repository.InterfaceClass
{
	public interface IEmailHelper
	{
		Task<Status> SendEmail(string userEmail,string body, string subject);
        Task<string> RenderToStringAsync(string viewName, object model);

    }
}
