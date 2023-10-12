using Microsoft.AspNetCore.Identity;

namespace test0000001.Models
{
	public class ApplicationUser:IdentityUser
	{
		public string? FirstName { get; set; }
        public string? LastName { get; set; }



		public DateTime DateOfBirth { get; set; }

		public string? Gender { get; set; }

		
		public string? ProfilePicture { get; set; }

		public string? Address { get; set; }
	
	}
}
