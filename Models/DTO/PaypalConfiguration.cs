using PayPal.Api;

namespace test0000001.Models.DTO
{
	public static class PaypalConfiguration
	{
		//Variables for storing the clientID and clientSecret key  
		private readonly static string ClientId = "AWHX_ZpW3EUyQpQD4q_koPq8uUZ_aXXnicPiFdqLJQX1NBHN0730PRjJK_qfH1VhfgIQpGuMS8GGOk7B";
		private readonly static string ClientSecret= "EJuT6_wKiycjFVQsavcPkx8upTn4ez7unNtFX3QLNzWy4bI5PXsWwom7eWCn7R1cfEn9FXM5AuvAOw5T";
		//Constructor  

		// getting properties from the web.config  
		public static Dictionary<string, string> GetConfig()
		{
			//return PayPal.Api.ConfigManager.Instance.GetProperties();
			return new Dictionary<string, string>() 
			{
				{ "clientId", "AWHX_ZpW3EUyQpQD4q_koPq8uUZ_aXXnicPiFdqLJQX1NBHN0730PRjJK_qfH1VhfgIQpGuMS8GGOk7B" },
				{ "clientSecret", "EJuT6_wKiycjFVQsavcPkx8upTn4ez7unNtFX3QLNzWy4bI5PXsWwom7eWCn7R1cfEn9FXM5AuvAOw5T" },
				{"mode","sandbox" }
			};
		}
		private static string GetAccessToken()
		{
			// getting accesstocken from paypal  
			string accessToken = new OAuthTokenCredential(ClientId, ClientSecret, GetConfig()).GetAccessToken();
			return accessToken;
		}
		public static APIContext GetAPIContext()
		{
			// return apicontext object by invoking it with the accesstoken  
			APIContext apiContext = new APIContext(GetAccessToken());
			apiContext.Config = GetConfig();
			return apiContext;
		}
	}

}
