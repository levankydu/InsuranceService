using Newtonsoft.Json;
using test0000001.Models.DTO.Country;
using test0000001.Repository.InterfaceClass;

namespace test0000001.Repository.ServiceClass
{
	public class CountryServices : ICountry
	{
		string countryAPI = "https://provinces.open-api.vn/api/";
		HttpClient client = new();
		public List<City> getCity()
		{
			var cities = JsonConvert.DeserializeObject<List<City>>(client.GetStringAsync(countryAPI + "?depth=1").Result);
			return cities!;
		}

		public List<District> getDistricts(string city)
		{
			var cities = JsonConvert.DeserializeObject<List<City>>(client.GetStringAsync(countryAPI + "?depth=2").Result)!.Where(l => l.code!.Equals(city)).FirstOrDefault();
			return cities!.districts!;
		}

		public List<Ward> getWards(string district)
		{
			var districts = JsonConvert.DeserializeObject<List<Ward>>(client.GetStringAsync(countryAPI + "w/").Result);

			return districts!.Where(l => l.district_code!.Equals(district)).ToList();
		}
	}
}
