using System.ComponentModel.DataAnnotations;

namespace test0000001.Models.LifeInsurance
{
    public enum DescriptionType
    {
        [Display(Name = "Yes/No")]
        yesNo,
        text
    }
}
