using test0000001.Models.DTO.Country;

namespace test0000001.Repository.InterfaceClass
{
	public interface ICountry
	{
		List<City> getCity();


		List<District> getDistricts(string city);

		List<Ward> getWards(string district);
	}
}
