using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Extensions;
using test0000001.Models;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class LifeInsuranceService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LifeInsuranceService(
            DatabaseContext dbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<Policy> GetAll()
        {
            return _dbContext.Policy
                    .Include(m => m.Duration)
                    .Where(m => m.InsuranceCategoryId.Equals(1));
        }

        public Policy? GetById(int? id)
        {
            return _dbContext.Policy
                    .Include(m => m.Duration)
                    .FirstOrDefault(m => m.Id.Equals(id));
        }

        public Policy? GetBySlug(string slug)
        {
            return _dbContext.Policy
                .Include(m => m.Duration)
                .FirstOrDefault(m => m.Slug != null && m.Slug.Equals(slug));
        }

        public IEnumerable<Policy> GetRecentCreated(int count)
        {
            return _dbContext.Policy
                    .AsNoTracking()
                    .OrderByDescending(m => m.Id)
                    .Take(count);
        }

        public IEnumerable<Duration> GetDurations(decimal val)
        {
            return _dbContext.Duration
                    .Where(m => m.PriceAmount.Equals(val));
        }

        public IEnumerable<Duration> GetDurationsInYear(decimal val)
        {
            return GetDurations(val)
                .Select(m => new Duration
                {
                    Id = m.Id,
                    PriceAmount = m.PriceAmount,
                    Term = m.Term / 12
                });
        }

        public bool Add(Policy policy)
        {
            _dbContext.Policy.Add(policy);
            return _dbContext.SaveChanges() > 0;
        }

        public bool Edit(Policy policy)
        {
            _dbContext.Entry(policy).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var model = GetById(id);
            if (model == null) return false;
            _dbContext.Policy.Remove(model);
            return _dbContext.SaveChanges() > 0;
        }

        // Handle Package with Session
        public Policy? SetPackage(string slug)
        {
            var package = GetBySlug(slug);
            _httpContextAccessor.HttpContext?.Session.Set(slug, package);
            return package;
        }
        public Policy? GetPackage(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<Policy>(slug);
        }
        public void ClearPackage(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(slug);
        }
        public Policy? SetPackage(int id)
        {
            var package = GetById(id);
            _httpContextAccessor.HttpContext?.Session.Set(id + "_LifeInsurance", package);
            return package;
        }
        public Policy? GetPackage(int id)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<Policy>(id + "_LifeInsurance");
        }
        public void ClearPackage(int id)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(id + "_LifeInsurance");
        }

        // Get package purchaser
        public ApplicationUser? GetUserById(string? id, bool asNoTracking = false)
        {
            return asNoTracking ?
                _dbContext.applicationUser?.AsNoTracking()
                        .FirstOrDefault(m => m.Id.Equals(id)) :
                _dbContext.applicationUser?.Find(id);
        }

        // Handle Package Purchaser with Session
        public void SetUser(string? id, ApplicationUser user)
        {
            if (id == null) return;
            string key = "User_" + id;
            _httpContextAccessor.HttpContext?.Session.Set(key, user);
        }
        public ApplicationUser? GetUser(string? id)
        {
            if (id == null) return null;
            string key = "User_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<ApplicationUser>(key);
        }
        public void ClearUser(string? id)
        {
            string key = "User_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(key);
        }

    }
}
