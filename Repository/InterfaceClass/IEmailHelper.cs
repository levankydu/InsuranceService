using Microsoft.AspNetCore.Mvc;
using InsuranceServices.Models.DTO;

namespace InsuranceServices.Repository.InterfaceClass
{
	public interface IEmailHelper
	{
		Task<Status> SendEmail(string userEmail,string body, string subject);
        Task<string> RenderToStringAsync(string viewName, object model);

    }
}
