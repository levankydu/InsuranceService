using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test0000001.Models
{
	[Table(name:"TbGuestMessage")]
	public class GuestMessage
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public string? Name { get; set; }

		public string? Email { get; set; }

		public string? PhoneNumber { get; set; }

		public string? Content { get; set; }

		public bool isSent { get; set; }=false;
		public bool isUser { get; set; }
		public string? AdminAnswer { get; set; } = null;
		public string? InsuranceType { get; set; }

	}
}
