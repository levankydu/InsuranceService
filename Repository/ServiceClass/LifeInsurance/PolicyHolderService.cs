using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Extensions;
using test0000001.Models;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class PolicyHolderService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PolicyHolderService(
            DatabaseContext dbContext,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<Policyholder> GetAll()
        {
            return _dbContext.Policyholder
                    .Include(m => m.User)
                    .Include(m => m.Policy)
                    .ThenInclude(p => p!.Duration)
                    .Include(m => m.LifeInsuredObject)
                    .Where(m => m.Policy != null && m.Policy.InsuranceCategoryId.Equals(1))
                    .ToList();
        }

        public IEnumerable<Policyholder> GetByUserId(string userId)
        {
            return _dbContext.Policyholder
                    .Include(m => m.User)
                    .Include(m => m.Policy)
                    .ThenInclude(p => p!.Duration)
                    .Include(m => m.LifeInsuredObject)
                    .Where(m => m.Policy != null && m.User != null &&
                        m.User.Id.Equals(userId) &&
                        m.Policy.InsuranceCategoryId.Equals(1))
                    .ToList();
        }

        public IEnumerable<Policyholder> GetByStatus(string status)
        {
            return _dbContext.Policyholder
                    .Where(m => m.Policy!= null && 
                        m.Policy.InsuranceCategoryId.Equals(1) && 
                        m.Status != null && 
                        m.Status.Equals(status))
                    .ToList();
        }

        public Policyholder? GetById(int? id, bool asNoTracking = false)
        {
            return asNoTracking ?
                _dbContext.Policyholder.AsNoTracking()
                        .FirstOrDefault(m => m.Id == id) :
                _dbContext.Policyholder.Find(id);
        }
        public Policyholder? GetById(int? id, bool includeInsuredObj = false, bool asNoTracking = true)
        {
            return includeInsuredObj ?
            _dbContext.Policyholder
                    .Include(m => m.LifeInsuredObject)
                    .FirstOrDefault(m => m.Id.Equals(id)) :
            _dbContext.Policyholder
                    .FirstOrDefault(m => m.Id.Equals(id));
        }

        public bool IsPackageActivated(int id)
        {
            var holder = GetById(id, asNoTracking: true);
            if (holder == null) return false;
            var status = holder.Status;
            if (status == null) return false;
            return status.Equals("Activated");
        }

        public bool Add(Policyholder policyHolder)
        {
            _dbContext.Policyholder.Add(policyHolder);
            return _dbContext.SaveChanges() > 0;
        }

        public bool Edit(Policyholder policyholder)
        {
            _dbContext.Entry(policyholder).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }

        public bool ApproveById(int id)
        {
            var model = _dbContext.Policyholder.Find(id);
            if (model == null) return false;
            model.Status = "Activated";
            model.ApprovedAt = DateTime.Now;
            return _dbContext.SaveChanges() > 0;
        }

        public bool RejectById(int id)
        {
            var model = _dbContext.Policyholder.Find(id);
            if (model == null) return false;
            model.Status = "Rejected";
            return _dbContext.SaveChanges() > 0;
        }

        public bool TerminateById(int id)
        {
            var model = _dbContext.Policyholder.Find(id);
            if (model == null) return false;
            model.Status = "Terminated";
            return _dbContext.SaveChanges() > 0;
        }

        public void SetPolicyHolder(string slug, Policyholder policyHolder)
        {
            _httpContextAccessor.HttpContext?.Session
                .Set(slug + "_PolicyHolder", policyHolder);
        }
        public Policyholder? GetPolicyHolder(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<Policyholder>(slug + "_PolicyHolder");
        }
        public void ClearPolicyHolder(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(slug + "_PolicyHolder");
        }

        public void SetPolicyHolder(int id, Policyholder policyHolder)
        {
            string key = "PolicyHolder_" + id;
            _httpContextAccessor.HttpContext?.Session
                .Set(key, policyHolder);
        }
        public Policyholder? GetPolicyHolder(int id)
        {
            string key = "PolicyHolder_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<Policyholder>(key);
        }
        public void ClearPolicyHolder(int id)
        {
            string key = "PolicyHolder_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(key);
        }

    }
}
