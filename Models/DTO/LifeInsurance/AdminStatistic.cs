namespace test0000001.Models.DTO.LifeInsurance
{
    public class AdminStatistic
    {
        public int PolicyHolderCount { get; set; }

        public int PolicyHolderPendingCount { get; set; }

        public int PolicyCount { get; set; }

        public int AppraisalQuesCount { get; set; }

        public List<DuePaymentDto>? DuePayments { get; set; }

    }
}
