using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Configuration;
using System.Security.Claims;
using System.Security.Policy;
using System.Text.Json.Serialization;
using test0000001.DB;
using test0000001.Models;
using test0000001.Repository.InterfaceClass;
using test0000001.Repository.ServiceClass;
using static System.Net.WebRequestMethods;
using test0000001.Repository.ServiceClass.HostedService;
using test0000001.Repository.ServiceClass.LifeInsurance;

var builder = WebApplication.CreateBuilder(args);

// Start Define all service in website

// Add services to the container.
builder.Services.AddControllersWithViews().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
});
//builder.Services.AddSession();


builder.Services.AddDbContext<DatabaseContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("url")));
builder.Services.AddScoped<ICountry, CountryServices>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHostedService<PolicyHolderHostedService>();

builder.Services.AddScoped<IInsuranceCategory, InsuranceCategoryServices>();
builder.Services.AddScoped<IDuration, DurationServices>();
builder.Services.AddScoped<IPolicy, PolicyServices>();
builder.Services.AddScoped<IEmailHelper, EmailServices>();
builder.Services.AddScoped<IUserAuthentication, UserServices>();


// KIEN ==> Life Insurance Services
builder.Services.AddScoped<AppraisalQuesService>();
builder.Services.AddScoped<AppraisalInfoService>();
builder.Services.AddScoped<LifeInsuranceService>();
builder.Services.AddScoped<LifeInsuredObjectService>();
builder.Services.AddScoped<LifeInsuredDossiersService>();
builder.Services.AddScoped<PaymentScheduleService>();
// KIEN ==> Supporting Services
builder.Services.AddScoped<PolicyHolderService>();
builder.Services.AddScoped<PolicyHolderViewService>();
builder.Services.AddScoped<FileHandlerService>();
builder.Services.AddScoped<MailingService>();
builder.Services.AddScoped<PdfHandlerService>();
builder.Services.AddSingleton(x =>
    new test0000001.Clients.PaypalForLifeClient(
		"AWHX_ZpW3EUyQpQD4q_koPq8uUZ_aXXnicPiFdqLJQX1NBHN0730PRjJK_qfH1VhfgIQpGuMS8GGOk7B",
		"EJuT6_wKiycjFVQsavcPkx8upTn4ez7unNtFX3QLNzWy4bI5PXsWwom7eWCn7R1cfEn9FXM5AuvAOw5T",
		//"ATJJo9jp9vmOFvFJ_qqmRIBVtkDODFUVacat2BRWIpk5UoFQgokh5GhJp67L7H0E_TdiViSpVRmqW2BL",
		//"ED4ujxp1lrG1xRF1CTTYVKIcfXzG3QwlXeDcn4goUQ0oqKe3m_0bvWGGyopSNL8vs1p8o1IqmXjxC-BZ",
		"Sandbox"
    )
);

// Thinh ==> Motor Insurance Services
builder.Services.AddScoped<IMotorInsurance, MotorInsuranceServices>();
//identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<DatabaseContext>().AddDefaultTokenProviders();

// Set the time of token forgot password validity
builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10));

builder.Services
	.AddAuthentication(options =>
	{
		options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		//options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
	})
	.AddCookie(opt =>
	{
		opt.LoginPath = "/Login/Welcome";
		opt.ExpireTimeSpan = TimeSpan.FromDays(1);
		opt.AccessDeniedPath = new PathString("/Account/AccessDenied");

	})
	.AddGoogle(GoogleDefaults.AuthenticationScheme, opts =>
{
    opts.ClientId = "1057076628569-h9uah1ovgr05s4gsl5kp1r46d4q9gjt2.apps.googleusercontent.com";
    opts.ClientSecret = "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";
    //opts.CallbackPath = "/Login/GoogleResponse";
    //opts.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //opts.SignInScheme = GoogleDefaults.AuthenticationScheme;
    opts.SaveTokens = true;
    opts.CorrelationCookie.SameSite = SameSiteMode.Lax;
});

//service cookie SamSite
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.OnAppendCookie = cookieContext =>
    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
    options.OnDeleteCookie = cookieContext =>
    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
});
void CheckSameSite(HttpContext httpContext, CookieOptions options)
{
    if (options.SameSite == SameSiteMode.None)
    {
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        if (MyUserAgentDetectionLib.DisallowsSameSiteNone(userAgent))
        {
            options.SameSite = SameSiteMode.Unspecified;
        }
    }
}

// End Define Service 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseCookiePolicy();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



public class MyUserAgentDetectionLib
{
    public static bool DisallowsSameSiteNone(string userAgent)
    {
        // Check if a null or empty string has been passed in, since this
        // will cause further interrogation of the useragent to fail.
        if (String.IsNullOrWhiteSpace(userAgent))
            return false;

        // Cover all iOS based browsers here. This includes:
        // - Safari on iOS 12 for iPhone, iPod Touch, iPad
        // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
        // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
        // All of which are broken by SameSite=None, because they use the iOS networking
        // stack.
        if (userAgent.Contains("CPU iPhone OS 12") ||
            userAgent.Contains("iPad; CPU OS 12"))
        {
            return true;
        }

        // Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
        // - Safari on Mac OS X.
        // This does not include:
        // - Chrome on Mac OS X
        // Because they do not use the Mac OS networking stack.
        if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
            userAgent.Contains("Version/") && userAgent.Contains("Safari"))
        {
            return true;
        }

        // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
        // and none in this range require it.
        // Note: this covers some pre-Chromium Edge versions, 
        // but pre-Chromium Edge does not require SameSite=None.
        if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
        {
            return true;
        }

        return false;
    }
}
