using System.Text;
using InsuranceServices.Models.DTO.LifeInsurance;

namespace InsuranceServices.Extensions
{
    public static class DateTimeExtensions
    {
        public static int GetCurrentAge(this DateTime datetime)
        {
            return DateTime.Now.Subtract(datetime).Days / 365;
        }

        public static bool IsDue(this DateTime datetime)
        {
            return DateTime.Compare(datetime, DateTime.Now) < 0;
        }
    }
}
