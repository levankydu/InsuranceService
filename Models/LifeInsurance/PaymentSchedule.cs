using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models.LifeInsurance
{
    [Table(name: "tbPaymentSchedule")]
    public class PaymentSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int PolicyHolderId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.NotDue;

        public int? PaymentId {  get; set; }

        public virtual Payment? Payment { get; set; }


        //public virtual Policyholder? PolicyHolder { get; set; }

        public virtual ApplicationUser? User { get; set; }

    }
}
