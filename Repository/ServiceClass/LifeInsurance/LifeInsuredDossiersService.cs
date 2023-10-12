using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Extensions;
using test0000001.Models.LifeInsurance;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class LifeInsuredDossiersService
    {
        private readonly DatabaseContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LifeInsuredDossiersService(DatabaseContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<AppraisalDossier> GetAll()
        {
            return _dbContext.AppraisalDossiers
                    .Where(m => m.InsuranceCategoryId.Equals(1));
        }

        public AppraisalDossier? GetById(int? id)
        {
            return _dbContext.AppraisalDossiers.Find(id);
        }

        public AppraisalDossier? GetByInsuredObjectId(int id)
        {
            return _dbContext.AppraisalDossiers
                .AsNoTracking()
                .FirstOrDefault(m => m.LifeInsuredObjectId.Equals(id));
        }

        public bool Add(AppraisalDossier dossier)
        {
            _dbContext.AppraisalDossiers.Add(dossier);
            return _dbContext.SaveChanges() > 0;
        }

        public bool Edit(AppraisalDossier dossier)
        {
            _dbContext.Entry(dossier).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }

        public void SetDossier(string slug, AppraisalDossier appraisalDossier)
        {
            _httpContextAccessor.HttpContext?.Session
                .Set(slug + "_LifeAppraisalDossier", appraisalDossier);
        }

        public AppraisalDossier? GetDossier(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<AppraisalDossier>(slug + "_LifeAppraisalDossier");
        }

        public void ClearDossier(string slug)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(slug + "_LifeAppraisalDossier");
        }

        public void SetDossier(int id, AppraisalDossier appraisalDossier)
        {
            string key = "LifeAppraisalDossier_" + id;
            _httpContextAccessor.HttpContext?.Session.Set(key, appraisalDossier);
        }

        public AppraisalDossier? GetDossier(int id)
        {
            string key = "LifeAppraisalDossier_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<AppraisalDossier>(key);
        }

        public void ClearDossier(int id)
        {
            string key = "LifeAppraisalDossier_" + id;
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(key);
        }

    }
}
