namespace InsuranceServices.Models.DTO
{
	public class SendEmail
	{
		public string? Email { get; set; }
		public string? ContentSend { get; set; }
		public string? Subject { get; set; } = "INSURANCE SERVICE- EMAIL RESPONE";
	}
}
