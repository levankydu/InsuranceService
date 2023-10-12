using System.ComponentModel.DataAnnotations;

namespace test0000001.Models.DTO
{
	public class Login
	{
		[Key]
		public string? Email { get; set; }

		public string? Password { get; set; }

		public bool KeepLogin { get; set; }

		
	}
}
