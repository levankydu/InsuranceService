using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models
{
    [Table(name:"tbDuration")]
    public class Duration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //public int DurationAmount { get; set; }

        [Required]
        public decimal PriceAmount { get; set; }

        [Required]
        [Display(Name = "Term (in month)")]
        public int Term { get; set; }
        public int TotalPeriod { get; set; } = 1;
    }
}
