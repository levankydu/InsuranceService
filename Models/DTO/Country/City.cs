namespace InsuranceServices.Models.DTO.Country
{
	public class City
	{
		public string? province_id { get; set; }
		public string? province_name { get; set; }


		public string? province_type { get; set; }

		public virtual List<District>? districts { get; set; }


	}
}
