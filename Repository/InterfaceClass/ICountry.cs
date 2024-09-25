using InsuranceServices.Models.DTO.Country;

namespace InsuranceServices.Repository.InterfaceClass
{
	public interface ICountry
	{
		List<City> getCity();


		List<District> getDistricts(string city);

		List<Ward> getWards(string district);
	}
}
