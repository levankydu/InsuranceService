using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models
{
    [Table(name:("tbPolicy"))]
    public class Policy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public decimal Premium { get; set; }

        [Required]
        public int InsuranceCategoryId { get; set; }
        
        [Required]
        [Display(Name = "Term")]
        public int DurationId { get; set; }

        public string? Image { get; set; } = string.Empty;

        public virtual InsuranceCategory? InsuranceCategory { get; set; }

        public virtual Duration? Duration { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
