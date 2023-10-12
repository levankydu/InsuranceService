using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace test0000001.Models.DTO
{
	public class ViewLogin
	{
        [Key]
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Name { get; set; }

        public string? ConfirmPassword { get; set; }

        public string? Role { get; set; }
		public bool KeepLogin { get; set; }
	}
}
