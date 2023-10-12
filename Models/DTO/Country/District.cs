namespace test0000001.Models.DTO.Country
{
	public class District
	{
		public string? name {  get; set; }

		public string? code { get; set; }

		public string? codename { get; set; }

		public string? division_type { get; set; }

		public string? short_codename { get; set; }

		public virtual List<Ward>? wards { get; set; }

	}
}
