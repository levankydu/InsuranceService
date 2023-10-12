using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Models.LifeInsurance;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class AppraisalQuesService
    {
        private readonly DatabaseContext _dbContext;

        public AppraisalQuesService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<AppraisalQues> GetByCateId(int insuranceCate)
        {
            return _dbContext.AppraisalQues
                .Where(m => m.InsuranceCategoryId.Equals(insuranceCate));
        }

        public AppraisalQues? GetById(int? id)
        {
            return _dbContext.AppraisalQues.Find(id);
        }

        public bool Add(AppraisalQues appraisalQues)
        {
            _dbContext.AppraisalQues.Add(appraisalQues);
            return _dbContext.SaveChanges() > 0;
        }

        public bool Edit(AppraisalQues appraisalQues)
        {
            _dbContext.Entry(appraisalQues).State = EntityState.Modified;
            return _dbContext.SaveChanges() > 0;
        }

        public bool Delete(int id)
        {
            var model = GetById(id);
            if (model == null) return false;
            _dbContext.AppraisalQues.Remove(model);
            return _dbContext.SaveChanges() > 0;
        }
    }
}
