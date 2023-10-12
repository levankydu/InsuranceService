using System.ComponentModel.DataAnnotations;

namespace test0000001.Models.DTO
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
