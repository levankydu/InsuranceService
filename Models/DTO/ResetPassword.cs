using System.ComponentModel.DataAnnotations;

namespace InsuranceServices.Models.DTO
{
    public class ResetPassword
    {
        [Required]
        public string? Password { get; set; }

        public string? ConfirmPassword { get; set; }

        public string? Email { get; set; }
        public string? Token { get; set; }
    }
}
