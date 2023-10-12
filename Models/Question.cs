using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models
{
    [Table(name: "tbQuestion")]
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InsuranceCategoryId { get; set; }
        public virtual InsuranceCategory? InsuranceCategory { get; set; }
        public string? Content { get; set; }
        //public bool IsActive { get; set; } = true;
    }
}
