using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models
{
    [Table(name: "tbPayment")]
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public int PolicyholderId { get; set; }
        public virtual Policyholder? Policyholder { get; set; }

        public string? Method { get; set; }

        public decimal Amount { get; set; }

        public int PaymentPeriod { get; set; } = 0;

        // KIEN add this
        public virtual LifeInsurance.PaymentSchedule? PaymentSchedule { get; set; }

    }
}
