using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Reflection.Emit;
using test0000001.Models;
using test0000001.Models.LifeInsurance;
//using Object = test0000001.Models.Object;
using test0000001.Models.DTO;
using test0000001.Models.DTO.HomeInsurance;

namespace test0000001.DB
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }


        public DbSet<ApplicationUser>? applicationUser { get; set; }
        public DbSet<DisbursementPayment>? disbursementPayment { get; set; }

        public DbSet<Duration> Duration { get; set; } = null!;

        public DbSet<InsuranceCategory>? insuranceCategory { get; set; }

        public DbSet<Payment>? payment { get; set; }

        public DbSet<Policy> Policy { get; set; } = null!;

        public DbSet<Policyholder> Policyholder { get; set; } = null!;

        public DbSet<LifeInsuredObject> LifeInsuredObject { get; set; } = null!;
        public DbSet<Home_Insurance> Home_Insurance { get; set; }

        public DbSet<CarInsuredObject> CarInsuredObject { get; set; } = null!;
        public DbSet<AppraisalQues> AppraisalQues { get; set; } = null!;

        public DbSet<AppraisalInfo> AppraisalInfos { get; set; } = null!;

        public DbSet<AppraisalDossier> AppraisalDossiers { get; set; } = null!;

        public DbSet<PaymentSchedule> PaymentSchedule { get; set; } = null!;
        public DbSet<GuestMessage> GuestMessage { get; set; } = null!;
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Photos> Photos { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Policyholder>(entity =>
            {
                entity.HasMany(o => o.Payments)
                    .WithOne(i => i.Policyholder)
                    .HasForeignKey(o => o.PolicyholderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(o => o.DisbursementPayments)
                    .WithOne(i => i.Policyholder)
                    .HasForeignKey(o => o.PolicyholderId)
                    .OnDelete(DeleteBehavior.Restrict);

                /*entity.HasMany(ph => ph.PaymentSchedules)
                    .WithOne()
                    .HasForeignKey(ps => ps.PolicyHolderId)
                    .OnDelete(DeleteBehavior.Restrict);*/
            });


            builder.Entity<LifeInsuredObject>(entity =>
            {
                entity.HasMany(o => o.AppraisalInfos)
                    .WithOne(i => i.LifeInsuredObject)
                    .HasForeignKey(o => o.LifeInsuredObjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.AppraisalDossier)
                    .WithOne(d => d.LifeInsuredObject)
                    .HasForeignKey<AppraisalDossier>(d => d.LifeInsuredObjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
