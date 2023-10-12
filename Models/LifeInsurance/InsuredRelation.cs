using System.ComponentModel.DataAnnotations;

namespace test0000001.Models.LifeInsurance
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
