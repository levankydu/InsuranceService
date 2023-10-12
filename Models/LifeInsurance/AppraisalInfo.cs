using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace test0000001.Models.LifeInsurance
{
    [Table(name: "tbAppraisalInfo")]
    public class AppraisalInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int InsuranceCategoryId { get; set; }

        [Required]
        public int LifeInsuredObjectId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DescriptionType DescriptionType { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public string Answer { get; set; } = string.Empty;

        [Display(Name = "Description Detail")]
        public string? DescriptionDetail { get; set; } = string.Empty;

        //[RequiredIf(otherProperty:"Answer", otherPropertyValue: "yes", AllowEmptyStrings = true, ErrorMessage = "This field is required")]
        public string? DetailAnswer { get; set; } = string.Empty;

        public virtual InsuranceCategory? InsuranceCategory { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual LifeInsuredObject? LifeInsuredObject { get; set; }

    }
}
