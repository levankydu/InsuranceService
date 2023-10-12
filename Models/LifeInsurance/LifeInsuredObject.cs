using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using test0000001.Attributes;

namespace test0000001.Models.LifeInsurance
{
    public class LifeInsuredObject
    {
        /*public Guid? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }*/

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Who does this insurance package applies to")]
        public InsuredRelation? InsuredRelation { get; set; }

        [Display(Name = "Other Detail")]
        public string? InsuredRelationDetail { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Insured Name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Citizen identification number (ID)")]
        public string? PID { get; set; } = string.Empty; // CCCD or CMND or Birth Cert. no.

        [DataType(DataType.Date), NotFutureDateValidation]
        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "ID issue Date")]
        public DateTime PidIssueDate { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "ID issue Place")]
        public string? PidIssuePlace { get; set; } = string.Empty;

        [DataType(DataType.Date), NotFutureDateValidation]
        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "This field is required")]
        public Gender? Gender {  get; set; }

        [Required(ErrorMessage = "This field is required")]
        [Display(Name = "Marital Status")]
        public MaritalStatus? MaritalStatus { get; set; }

        [Required(ErrorMessage = "This field is required")]
        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$",
         ErrorMessage = "Input must be in the Email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        public string? Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required")]
        public string? Address { get; set; }

        [Required]
        public Guid AppraisalManifestId { get; set; } = Guid.NewGuid();

        [Required]
        public int PolicyholderId { get; set; }

        public virtual Policyholder? Policyholder { get; set; }

        public virtual ICollection<AppraisalInfo>? AppraisalInfos { get; set; }

        public virtual AppraisalDossier? AppraisalDossier {  get; set; }

    }
}
