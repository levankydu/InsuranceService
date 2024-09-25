using InsuranceServices.Models.LifeInsurance;

namespace InsuranceServices.Models.DTO.LifeInsurance
{
    public class PaymentScheduleDto
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;
        
        public int PolicyHolderId { get; set; }
        
        public string? UserId { get; set; }
        
        public decimal Amount { get; set; }
        
        public DateTime DueDate { get; set; }
        
        public PaymentStatus Status { get; set; }

        public int? PaymentId { get; set; }

        public DateTime? PaymentDate { get; set; }
    }
}
