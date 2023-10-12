using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models.LifeInsurance
{
    [Table(name: "tbAppraisalDossier")]
    public class AppraisalDossier
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
        [Display(Name = "Citizen Identification")]
        public string Identification { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Health Certification")]
        public string HealthCertification { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? OtherDescription { get; set; }

        [Display(Name = "Other File")]
        public string? Other { get; set; } = string.Empty;

        public virtual InsuranceCategory? InsuranceCategory { get; set; }

        public virtual LifeInsuredObject? LifeInsuredObject { get; set; }

        public virtual ApplicationUser? User { get; set; }

    }
}
