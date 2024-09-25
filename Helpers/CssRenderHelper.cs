using Microsoft.AspNetCore.Mvc.Rendering;
using InsuranceServices.Models.LifeInsurance;

namespace InsuranceServices.Helpers
{
    public static class CssRenderHelper
    {
        public static string? PolicyStatusCss(this IHtmlHelper htmlHelper, string? status)
        {
            if (status == null) return null;
            return status switch
            {
                "Pending" or "0" => "bg-warning",
                "Rejected" => "bg-danger",
                "Activated" => "bg-success",
                "Terminated" => "bg-dark",
                _ => "bg-secondary"
            };
        }
        
        public static string? PaymentStatusCss(this IHtmlHelper htmlHelper, PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.NotDue => "bg-secondary",
                PaymentStatus.Due => "bg-danger",
                PaymentStatus.Paid or PaymentStatus.Success => "bg-success",
                _ => "bg-dark"
            };
        }
    }
}
