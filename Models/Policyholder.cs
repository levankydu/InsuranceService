using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Globalization;
using NuGet.Packaging;
using test0000001.Models.LifeInsurance;
using test0000001.Attributes;
using System.Text.RegularExpressions;

namespace test0000001.Models
{
    [Table(name: "tbPolicyholder")]
    public class Policyholder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public int PolicyId { get; set; }
        public virtual Policy? Policy { get; set; }
        [Required]
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public string? Object { get; set; }
        public string? Question { get; set; }

        public DateTime ApprovedAt { get; set; }
        public virtual LifeInsuredObject? LifeInsuredObject { get; set; }

        public int? CarInsuredObjectId { get; set; }
        public virtual CarInsuredObject? CarInsuredObject { get; set; }
        //public virtual ICollection<PaymentSchedule>? PaymentSchedules { get; set; }

        [DataType(DataType.Date), Required, StartDateValidation]
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public DateTime ApprovedDay { get; set; }
        public string? Status { get; set; } //Pending, Waiting For Payment, Activated, Rejected, Expired
        public List<Payment>? Payments { get; set; }
        public List<DisbursementPayment>? DisbursementPayments { get; set; }
        //public bool IsActive { get; set; }
        public decimal EachPeriodPrice()
        {
          
            return this.Policy!.Premium / this.Policy.Duration!.TotalPeriod;
        }
        public int MonthPerPeriod()
        {
            if (this.Policy.Duration.TotalPeriod == null || this.Policy.Duration.TotalPeriod == 0)
                return 0;
            return this.Policy!.Duration!.Term / this.Policy.Duration.TotalPeriod;
        }
        public static Dictionary<string, string> ConvertObject(string obj)
        {
            var result = new Dictionary<string, string>();
            if (obj != null)
            {
                var tempDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(obj);
                if (tempDict!.ContainsKey("purpose") && tempDict!["purpose"] != null)
                {
                    tempDict.Remove("purpose");
                }
                foreach (var item in tempDict!)
                {
                    string[] tempArr = item.Key.Split('_');
                    List<string> words = new List<string>();
                    foreach (var word in tempArr)
                    {
                        char[] a = word.ToCharArray();
                        a[0] = char.ToUpper(a[0]);
                        words.Add(new string(a));
                    }
                    result[string.Join(" ", words.ToArray())] = item.Value;
                }
            }
            return result!;
        }
        public static string ConvertToKey(string key)
        {
            if (!string.IsNullOrEmpty(key)) {
                key = Regex.Replace(key, "[^a-zA-Z0-9]+", "_");
                key = key.ToLower();
                string[] tempArr = key.Split(" ");
                key = string.Join("_", tempArr);
            }
            return key!;
        }
        public Payment? LastestPayment()
        {
            if (this.Payments!.Count() > 0 && this.Payments != null)
            {
                return this.Payments!.OrderByDescending(p => p.Id).FirstOrDefault();
            }
            return null;
        }
        public string StatusColor()
        {
            return this.Status == "Activated" ? "success" : (this.Status == "Rejected" || this.Status == "Expired" ? "danger" : "warning");
        }
        public DateTime TimeOfNextCharge()
        {
            if (this.StartDay != DateTime.MinValue || this.EndDay != DateTime.MinValue) {
                Payment? lastPaid = this.LastestPayment();
                var periodCount = lastPaid == null ? 1 : lastPaid.PaymentPeriod;
                if (periodCount <= this.Policy!.Duration!.TotalPeriod)
                {
                    return this.StartDay.AddMonths(this.MonthPerPeriod() * periodCount);
                }
            }
            return DateTime.MinValue;
        }
    }
}
