using System.ComponentModel.DataAnnotations;

namespace test0000001.Models.DTO
{
	public class Registration
	{
		[Key]
		public string? Email { get; set; }
		public string? FirstName { get; set; }
        public string? LastName { get; set; }



        public string? Password { get; set; }

		public string? PasswordConfirm { get; set; }

		public string? Role { get; set; }

	}
}
