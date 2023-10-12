using System.ComponentModel.DataAnnotations;

namespace test0000001.Models.LifeInsurance
{
    public enum Gender
    {
        Male,
        Female,
        [Display(Name = "Not show")]
        Other
    }
}
