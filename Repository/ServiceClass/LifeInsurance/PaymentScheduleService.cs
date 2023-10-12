using Microsoft.EntityFrameworkCore;
using test0000001.DB;
using test0000001.Extensions;
using test0000001.Models;
using test0000001.Models.DTO.LifeInsurance;
using test0000001.Models.LifeInsurance;

namespace test0000001.Repository.ServiceClass.LifeInsurance
{
    public class PaymentScheduleService
    {
        private readonly DatabaseContext _dbContext;
        private readonly PolicyHolderService _policyHolder;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentScheduleService(
            DatabaseContext dbContext,
            PolicyHolderService policyHolder,
            IHttpContextAccessor httpContextAccessor
            )
        {
            _dbContext = dbContext;
            _policyHolder = policyHolder;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<PaymentSchedule> GetAll()
        {
            return _dbContext.PaymentSchedule.ToList();
        }

        public PaymentSchedule? GetById(int? id, bool asNoTracking = false)
        {
            return asNoTracking ?
                _dbContext.PaymentSchedule
                    .AsNoTracking()
                    .FirstOrDefault(m => m.Id == id) :
                _dbContext.PaymentSchedule.Find(id);
        }

        public List<PaymentScheduleDto> GetByPolicyHolderId(int id)
        {
            return _dbContext.PaymentSchedule
                .AsNoTracking()
                .GroupJoin(_dbContext.payment!,
                    s => s.PaymentId,
                    p => p.Id,
                    (s, p) => new { s, p })
                .SelectMany(x => x.p.DefaultIfEmpty(),
                    (x, p) => new PaymentScheduleDto
                    {
                        Id = x.s.Id,
                        Description = x.s.Description,
                        PolicyHolderId = x.s.PolicyHolderId,
                        UserId = x.s.UserId,
                        Amount = x.s.Amount,
                        DueDate = x.s.DueDate,
                        Status = x.s.Status,
                        PaymentId = x.s.PaymentId,
                        PaymentDate = p != null ? p.CreatedAt : null
                    })
                .Where(m => m.PolicyHolderId.Equals(id))
                .ToList();
        }

        public List<DuePaymentCountDto> GetDueCounts()
        {
            return _dbContext.PaymentSchedule
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => _policyHolder.IsPackageActivated(m.PolicyHolderId))
                .Where(m => m.DueDate.IsDue() &&
                        !m.Status.Equals(PaymentStatus.Paid))
                .GroupBy(m => m.PolicyHolderId)
                .Select(group => new DuePaymentCountDto
                {
                    PolicyHolderId = group.Key,
                    DueCount = group.Count()
                }).ToList();
        }

        public List<DuePaymentCountDto> GetDueCountsByUserId(string userId)
        {
            return _dbContext.PaymentSchedule
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => m.UserId != null &&
                        m.UserId.Equals(userId) &&
                        _policyHolder.IsPackageActivated(m.PolicyHolderId))
                .Where(m => m.DueDate.IsDue() &&
                        !m.Status.Equals(PaymentStatus.Paid))
                .GroupBy(m => m.PolicyHolderId)
                .Select(group => new DuePaymentCountDto
                {
                    PolicyHolderId = group.Key,
                    DueCount = group.Count()
                }).ToList();
        }
        public List<DuePaymentDto> GetDuePayments()
        {
            return _dbContext.PaymentSchedule
                .AsNoTracking()
                .AsEnumerable()
                .Where(m => _policyHolder.IsPackageActivated(m.PolicyHolderId))
                .Where(m => m.DueDate.IsDue() &&
                        !m.Status.Equals(PaymentStatus.Paid))
                .Join(_policyHolder.GetAll(),
                    o => o.PolicyHolderId,
                    i => i.Id,
                    (o, i) => new DuePaymentDto
                    {
                        PolicyHolderId = o.PolicyHolderId,
                        PolicyName = i.Policy?.Name,
                        PolicyHolderName = i.User?.FirstName + " " + i.User?.LastName,
                        InsuredName = i.LifeInsuredObject?.Name,
                        Term = i.Policy?.Duration?.Term / 12,
                        StartDate = i.StartDay,
                        OverdueDays = (DateTime.Today - o.DueDate).Days,
                        DueAmount = o.Amount
                    })
                .ToList();
        }



        public bool Add(PaymentSchedule schedule)
        {
            _dbContext.PaymentSchedule.Add(schedule);
            return _dbContext.SaveChanges() > 0;
        }

        public bool AddRange(IEnumerable<PaymentSchedule> schedules)
        {
            _dbContext.PaymentSchedule.AddRange(schedules);
            return _dbContext.SaveChanges() > 0;
        }

        public bool SetPaymentToPaid(int id, int paymentId)
        {
            var model = GetById(id);
            if (model == null) return false;
            model.Status = PaymentStatus.Paid;
            model.PaymentId = paymentId;
            return _dbContext.SaveChanges() > 0;
        }

        public bool AddPayment(Payment payment)
        {
            _dbContext.payment?.Add(payment);
            return _dbContext.SaveChanges() > 0;
        }

        public Payment? GetPaymentById(int id, bool asNoTracking = false)
        {
            return asNoTracking ?
                _dbContext.payment?
                    .AsNoTracking()
                    .FirstOrDefault(m => m.Id.Equals(id)) :
                _dbContext.payment?.Find(id);
        }

        public PaymentSchedule? GetNextPayment(int policyHolderId, bool asNoTracking = false)
        {
            return asNoTracking ?
                _dbContext.PaymentSchedule?
                    .AsNoTracking()
                    .Where(m => m.PolicyHolderId.Equals(policyHolderId) &&
                        m.Status.Equals(PaymentStatus.NotDue))
                    .OrderBy(m => m.Id)
                    .FirstOrDefault() :
                _dbContext.PaymentSchedule?
                    .Where(m => m.PolicyHolderId.Equals(policyHolderId) &&
                        m.Status.Equals(PaymentStatus.NotDue))
                    .OrderBy(m => m.Id)
                    .FirstOrDefault();
        }

        public void SetPaypalPaymentObject(int id, int packageId, PaypalPaymentDto paypalPaymentDto)
        {
            string key = $"PaypalPaymentDto_{packageId}_{id}";
            _httpContextAccessor.HttpContext?.Session.Set(key, paypalPaymentDto);
        }

        public PaypalPaymentDto? GetPaypalPaymentObject(int id, int packageId)
        {
            string key = $"PaypalPaymentDto_{packageId}_{id}";
            var session = _httpContextAccessor.HttpContext?.Session;
            return session?.Get<PaypalPaymentDto>(key);
        }

        public void ClearPaypalPaymentObject(int id, int packageId)
        {
            string key = $"PaypalPaymentDto_{packageId}_{id}";
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Remove(key);
        }
    }
}
