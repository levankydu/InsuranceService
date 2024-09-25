using System.ComponentModel.DataAnnotations;

namespace InsuranceServices.Models.DTO
{
	public class Login
	{
		[Key]
		public string? Email { get; set; }

		public string? Password { get; set; }

		public bool KeepLogin { get; set; }

		
	}
}
