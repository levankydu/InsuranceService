namespace test0000001.Models.DTO.Country
{
	public class City
	{
		public string? name { get; set; }

		public string? code { get; set; }

		public string? codename { get; set; }

		public int phone_code { get; set; }

		public virtual List<District>? districts { get; set; }


	}
}
