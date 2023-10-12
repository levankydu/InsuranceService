using Microsoft.AspNetCore.Mvc.Rendering;
using test0000001.Extensions;
using test0000001.Models.LifeInsurance;

namespace test0000001.Helpers
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
