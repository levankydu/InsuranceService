using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace test0000001.Helpers
{
    public static class DescriptionHelper
    {
        public static string GetShortDescription(this IHtmlHelper htmlHelper, string? description)
        {
            if (description == null) return string.Empty;
            string pattern = @"^<p.*?>(.*?)<\/p>";
            var result = Regex.Match(description, pattern).Groups[1].Value;
            return !string.IsNullOrEmpty(result) ? result : description;
        }
    }
}
