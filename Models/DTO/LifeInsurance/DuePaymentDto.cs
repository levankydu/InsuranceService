namespace test0000001.Models.DTO.LifeInsurance
{
    public class DuePaymentDto
    {
        public int? PolicyHolderId { get; set; }

        public string? PolicyName {  get; set; }

        public string? PolicyHolderName { get; set; }

        public string? InsuredName { get; set; }

        public int? Term {  get; set; }

        public DateTime StartDate { get; set; }

        public int? OverdueDays { get; set; }

        public decimal? DueAmount { get; set; }

    }
}
