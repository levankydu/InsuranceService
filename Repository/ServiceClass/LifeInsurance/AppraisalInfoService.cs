using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Extensions;
using test0000001.Models.DTO.LifeInsurance;
using test0000001.Models.LifeInsurance;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class AppraisalInfoService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppraisalInfoService(DatabaseContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<AppraisalInfo> GetAll()
        {
            return _dbContext.AppraisalInfos
                    .Where(m => m.InsuranceCategoryId.Equals(1));
        }

        public AppraisalInfo? GetById(int? id)
        {
            return _dbContext.AppraisalInfos.Find(id);
        }

        public List<AppraisalInfo> GetByInsuredObjectId(int id)
        {
            return _dbContext.AppraisalInfos
                    .AsNoTracking()
                    .Where(m => m.LifeInsuredObjectId.Equals(id))
                    .ToList();
        }

        public bool Add(AppraisalInfo appraisalInfo)
        {
            _dbContext.AppraisalInfos.Add(appraisalInfo);
            return _dbContext.SaveChanges() > 0;
        }

        public bool AddRange(IEnumerable<AppraisalInfo> appraisalList)
        {
            _dbContext.AppraisalInfos.AddRange(appraisalList);
            return _dbContext.SaveChanges() > 0;
        }

        public bool Edit(AppraisalInfo appraisalInfo)
        {
            _dbContext.Entry(appraisalInfo).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }

        public void SetAppraisalInfos(string slug, AppraisalInfosDto appraisalInfos)
        {
            _httpContextAccessor.HttpContext?.Session
                .Set(slug + "_LifeAppraisalInfos", appraisalInfos);
        }
        public AppraisalInfosDto? GetAppraisalInfos(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<AppraisalInfosDto>(slug + "_LifeAppraisalInfos");
        }
        public void ClearAppraisalInfos(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(slug + "_LifeAppraisalInfos");
        }

        public void SetAppraisalInfos(int id, AppraisalInfosDto appraisalInfos)
        {
            string key = "LifeAppraisalInfos_" + id;
            _httpContextAccessor.HttpContext?.Session.Set(key, appraisalInfos);
        }
        public AppraisalInfosDto? GetAppraisalInfos(int id)
        {
            string key = "LifeAppraisalInfos_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<AppraisalInfosDto>(key);
        }
        public void ClearAppraisalInfos(int id)
        {
            string key = "LifeAppraisalInfos_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(key);
        }
    }
}
