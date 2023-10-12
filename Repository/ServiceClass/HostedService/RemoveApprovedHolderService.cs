using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using test0000001.DB;

namespace test0000001.Repository.ServiceClass.HostedService
{
    public class PolicyHolderHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public PolicyHolderHostedService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var currentTime = DateTime.Now;
            var scope = _serviceScopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var email = scope.ServiceProvider.GetService<EmailServices>();

            while (!stoppingToken.IsCancellationRequested)
            {
                //if (currentTime.Hour == 4 && currentTime.Minute == 30)
                //{
                    var expired = db.Policyholder.Where(p => p.EndDay < currentTime && p.EndDay != DateTime.MinValue).ToList();
                    if (expired.Count() > 0)
                    {
                        expired.ForEach(e => e.Status = "Expired");
                    }

                    var notPaids = db.Policyholder.Where(p => p.Status == "Waiting For Payment" && p.Payments!.Count() == 0).ToList();
                    if (notPaids.Count() > 0) {
                        notPaids.ForEach(p => {
                            if (p.ApprovedDay != DateTime.MinValue && p.ApprovedDay.AddDays(3).Day < currentTime.Day) { 
                                db.Policyholder.Remove(p);
                            }
                        });
                    }

                    var requirePay = db.Policyholder.Include(p => p.Policy!.Duration).Include(p=>p.Payments).AsEnumerable().Where(p => p.Status == "Activated" && p.TimeOfNextCharge() != DateTime.MinValue && p.TimeOfNextCharge().Day == currentTime.Day && p.TimeOfNextCharge().Month == currentTime.Month && p.TimeOfNextCharge().Year == currentTime.Year).ToList();
                    if (requirePay.Count() > 0) { 
                        requirePay.ForEach(p => p.Status = "Waiting For Payment");
                    }

                    await db.SaveChangesAsync();

                    var nearliesPay = db.Policyholder.Include(p => p.User).Include(p => p.Policy!.Duration).Include(p => p.Payments).AsEnumerable().Where(p => p.Status == "Activated" && p.TimeOfNextCharge() != DateTime.MinValue && p.TimeOfNextCharge().AddDays(7).Day == currentTime.Day).ToList();
                    if (nearliesPay.Count() > 0) {
                        foreach (var item in nearliesPay) {
                            await email!.SendEmail(item.User!.Email, await email.RenderToStringAsync("../EmailTemplate/RequirePayment", item), "Require Payment");
                        }
                    }
                //}

                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
