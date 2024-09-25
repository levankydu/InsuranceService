using Microsoft.AspNetCore.Mvc.Rendering;
using InsuranceServices.Extensions;
using InsuranceServices.Models.LifeInsurance;

namespace InsuranceServices.Helpers
{
    public static class PaymentStatusHelper
    {
        public static PaymentStatus CurrentPaymentStatus(this IHtmlHelper htmlHelper, DateTime dueDate, PaymentStatus status)
        {
            if (status.Equals(PaymentStatus.Paid)) return status;
            return dueDate.IsDue() ? 
                    PaymentStatus.Due :
                    PaymentStatus.NotDue;
        }
    }
}
