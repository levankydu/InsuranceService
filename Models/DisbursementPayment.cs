using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models
{
    [Table(name: "tbDisbursementPayment")]
    public class DisbursementPayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PolicyholderId { get; set; }

        public virtual Policyholder? Policyholder { get; set; }

		public DateTime CreatedAt { get; set; }

        public string? Method { get; set; }

        public decimal Amount { get; set; }

        public string? Status { get; set; }
	
    }
}
