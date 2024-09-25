using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using InsuranceServices.Models.DTO.Country;
using InsuranceServices.Repository.InterfaceClass;

namespace InsuranceServices.Repository.ServiceClass
{
	public class CountryServices : ICountry
	{
		string countryAPI = "https://vapi.vnappmob.com/";
		HttpClient client = new HttpClient();

		public List<City> getCity()
		{
			try
			{
				var responseString = client.GetStringAsync(countryAPI + "api/province/").Result;
				if (string.IsNullOrWhiteSpace(responseString))
				{
					Console.WriteLine("Phản hồi từ API trống hoặc null.");
					return new List<City>();
				}

				var apiResponse = JsonConvert.DeserializeObject<ApiProvinceResponse>(responseString);
				if (apiResponse?.ProvincesResults == null)
				{
					Console.WriteLine("'results' là null trong phản hồi từ API.");
					return new List<City>();
				}

				return apiResponse.ProvincesResults;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Có lỗi xảy ra khi lấy hoặc giải mã dữ liệu: " + ex.Message);
				return new List<City>();
			}
		}

		public List<District> getDistricts(string provinceId)
		{
			try
			{
				var responseString = client.GetStringAsync(countryAPI + "api/province/district/" + provinceId).Result;
				if (string.IsNullOrWhiteSpace(responseString))
				{
					Console.WriteLine("Phản hồi từ API trống hoặc null.");
					return new List<District>();
				}

				var apiResponse = JsonConvert.DeserializeObject<ApiDistrictResponse>(responseString);
				if (apiResponse?.DistrictsResults == null)
				{
					Console.WriteLine("'results' là null trong phản hồi từ API.");
					return new List<District>();
				}

				return apiResponse.DistrictsResults;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Có lỗi xảy ra khi lấy hoặc giải mã dữ liệu: " + ex.Message);
				return new List<District>();
			}
		}

		public List<Ward> getWards(string districtId)
		{
			try
			{
				var responseString = client.GetStringAsync(countryAPI + "api/province/ward/" + districtId).Result;
				if (string.IsNullOrWhiteSpace(responseString))
				{
					Console.WriteLine("Phản hồi từ API trống hoặc null.");
					return new List<Ward>();
				}

				var apiResponse = JsonConvert.DeserializeObject<ApiWardResponse>(responseString);
				if (apiResponse?.WardsResult == null)
				{
					Console.WriteLine("'results' là null trong phản hồi từ API.");
					return new List<Ward>();
				}

				return apiResponse.WardsResult;
			}
			catch (Exception ex)
			{
				Console.WriteLine("Có lỗi xảy ra khi lấy hoặc giải mã dữ liệu: " + ex.Message);
				return new List<Ward>();
			}
		}
	}

	public class ApiProvinceResponse
	{
		[JsonProperty("results")]
		public List<City>? ProvincesResults { get; set; }
	}

	public class ApiDistrictResponse
	{
		[JsonProperty("results")]
		public List<District>? DistrictsResults { get; set; }
	}

	public class ApiWardResponse
	{
		[JsonProperty("results")]
		public List<Ward>? WardsResult { get; set; }
	}
}
