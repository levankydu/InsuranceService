using test0000001.Models.LifeInsurance;

namespace test0000001.Models.DTO.LifeInsurance
{
    public class PaypalPaymentDto
    {
        public int PaymentScheduleId {  get; set; }

        public int PolicyHolderId { get; set; }

        public PackageOverviewDto? PackageOverview { get; set; }

        public PaymentSchedule? PaidItem { get; set; }

        public PaymentSchedule? NextPaidItem { get; set; }

        public Payment? Payment { get; set; }

    }
}
