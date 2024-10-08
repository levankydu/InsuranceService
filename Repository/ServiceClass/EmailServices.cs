﻿using InsuranceServices.Repository.InterfaceClass;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using static InsuranceServices.Models.Policyholder;
using InsuranceServices.Models.DTO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InsuranceServices.Repository.ServiceClass
{
    public class EmailServices : IEmailHelper
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        
        public EmailServices(IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }
        public async Task<Status> SendEmail(string userEmail, string body, string subject)
        {
            var status = new Status();

			MailMessage mailMessage = new("leemarkyr@gmail.com", userEmail)
			{
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			SmtpClient client = new("smtp.gmail.com")
			{
				Port = 587,
				Credentials = new NetworkCredential("leemarkyr@gmail.com", "sord seog fene rqai"),
				EnableSsl = true
			};

			try
            {
                client.Send(mailMessage);
                status.StatusMessage = "PLease check your email , this will expired at last 10 hours";
                status.StatusCode = 1;
                return status;
            }
            catch (Exception ex)
            {
                status.StatusMessage = ex.Message;
                status.StatusCode = 0;
                return status;
            }
        }

        public async Task<string> RenderToStringAsync(string viewName, object model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());

            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(actionContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.ToString();
            }
        }
    }
}
