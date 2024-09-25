using System.ComponentModel.DataAnnotations;

namespace InsuranceServices.Models.LifeInsurance
{
    public enum InsuredRelation
    {
        self,

        child,

        [Display(Name = "Wife/Husband")]
        wifeHusband,

        parent,

        other
    }
}
