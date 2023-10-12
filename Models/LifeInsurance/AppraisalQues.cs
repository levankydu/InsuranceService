using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models.LifeInsurance
{
    [Table(name: "tbAppraisalQues")]
    public class AppraisalQues
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Display(Name = "CateID")]
        public int InsuranceCategoryId { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Type")]
        public DescriptionType DescriptionType { get; set; } = DescriptionType.yesNo;

        [Display(Name = "Description Detail")]
        public string? DescriptionDetail { get; set; } = string.Empty;

        public virtual InsuranceCategory? InsuranceCategory { get; set; }

    }
}
