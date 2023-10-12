using test0000001.DB;
using test0000001.Models;
using Microsoft.EntityFrameworkCore;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Repository.ServiceClass
{
    public class PolicyServices : IPolicy
    {
        private DatabaseContext db;

        public PolicyServices(DatabaseContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<Policy>> GetAllPolicy()
        {
            return await db.Policy!.ToListAsync();
        }

        public async Task<IEnumerable<Policy>> GetPolicyByCategory(int categoryId)
        {
            if (categoryId == 0)
            {
                // Lấy tất cả bảo hiểm nếu không có categoryId được chỉ định
                var policys = await db.Policy!.ToListAsync();
                return policys;
            }
            else
            {
                // Lấy bảo hiểm theo categoryId
                var policyByCategary = await db.Policy!.Where(p => p.InsuranceCategory!.Equals(categoryId)).ToListAsync();
                return policyByCategary;
            } 
        }
        public async Task<Policy> GetPolicyById(int id)
        {
            var policy = db.Policy!.SingleOrDefaultAsync(p => p.Id == id);
            if (policy != null)
            {
                return await policy;
            }
            else
            {
                return null!;
            }
        }

        public async Task<bool> AddPolicy(Policy newPolicy)
        {
            Duration? duration = db.Duration.Find(newPolicy.DurationId);
            newPolicy.Premium = duration!.PriceAmount;
            await db.Policy!.AddAsync(newPolicy);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EditPolicy(Policy editPolicy)
        {
            var policy = await db.Policy!.SingleOrDefaultAsync(u => u.Id!.Equals(editPolicy.Id));
            Duration? duration = db.Duration.Find(editPolicy.DurationId);
            if (policy != null)
            {
                policy.InsuranceCategory = editPolicy.InsuranceCategory;
                policy.Duration = editPolicy.Duration;
                policy.Premium = duration!.PriceAmount;
                policy.Name = editPolicy.Name;
                policy.Description = editPolicy.Description;
                await db.SaveChangesAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeletePolicy(int id)
        {
            var policy = await db.Policy!.SingleOrDefaultAsync(p => p.Id.Equals(id));
            if (policy == null)
            {
                return false;
            }
            db.Policy!.Remove(policy);
            await db.SaveChangesAsync();
            return true;
        }

       
    }
}
