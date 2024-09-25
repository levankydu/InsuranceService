using System.ComponentModel.DataAnnotations;

namespace InsuranceServices.Models.LifeInsurance
{
    public enum Gender
    {
        Male,
        Female,
        [Display(Name = "Not show")]
        Other
    }
}
