using System.ComponentModel.DataAnnotations;

namespace InsuranceServices.Models.LifeInsurance
{
    public enum DescriptionType
    {
        [Display(Name = "Yes/No")]
        yesNo,
        text
    }
}
