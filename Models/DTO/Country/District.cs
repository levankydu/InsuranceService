namespace InsuranceServices.Models.DTO.Country
{
	public class District
	{
		public string? district_id {  get; set; }

		public string? district_name { get; set; }

		public string? district_type { get; set; }

		public string? province_id { get; set; }

		public virtual List<Ward>? wards { get; set; }

	}
}