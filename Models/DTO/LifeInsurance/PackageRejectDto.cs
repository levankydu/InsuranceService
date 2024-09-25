using System.ComponentModel.DataAnnotations;

namespace InsuranceServices.Models.DTO.LifeInsurance
{
    public class PackageRejectDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; } = string.Empty;

    }
}
