using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Extensions;
using test0000001.Models.LifeInsurance;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class LifeInsuredObjectService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LifeInsuredObjectService(DatabaseContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<LifeInsuredObject> GetAll()
        {
            return _dbContext.LifeInsuredObject.ToList();
        }

        public LifeInsuredObject? GetById(int? id)
        {
            return _dbContext.LifeInsuredObject.Find(id);
        }
        
        public LifeInsuredObject? GetById(int? id, bool asNoTracking = false)
        {
            return asNoTracking ?
                _dbContext.LifeInsuredObject.AsNoTracking()
                        .FirstOrDefault(m => m.Id.Equals(id)) :
                _dbContext.LifeInsuredObject.Find(id);
        }

        public LifeInsuredObject? GetByPolicyHolderId(int id)
        {
            return _dbContext.LifeInsuredObject
                .AsNoTracking()
                .Where(m => m.PolicyholderId.Equals(id))
                .FirstOrDefault();
        }

        public bool Add(LifeInsuredObject insuredObject)
        {
            _dbContext.LifeInsuredObject.Add(insuredObject);
            return _dbContext.SaveChanges() > 0;
        }

        public bool Edit(LifeInsuredObject insuredObject)
        {
            _dbContext.Entry(insuredObject).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }

        public void SetInsuredObject(string slug, LifeInsuredObject insuredObj)
        {
            _httpContextAccessor.HttpContext?.Session
                .Set(slug + "_LifeInsuredObj", insuredObj);
        }

        public LifeInsuredObject? GetInsuredObject(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<LifeInsuredObject>(slug + "_LifeInsuredObj");
        }

        public void ClearInsuredObject(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(slug + "_LifeInsuredObj");
        }
        
        public void SetInsuredObject(int id, LifeInsuredObject insuredObj)
        {
            string key = "LifeInsuredObj_" + id;
            _httpContextAccessor.HttpContext?.Session.Set(key, insuredObj);
        }

        public LifeInsuredObject? GetInsuredObject(int id)
        {
            string key = "LifeInsuredObj_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<LifeInsuredObject>(key);
        }

        public void ClearInsuredObject(int id)
        {
            string key = "LifeInsuredObj_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(key);
        }

    }
}
