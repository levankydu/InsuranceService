using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace test0000001.Helpers
{
    public static class FileNameHelper
    {
        public static string GetFileName(this IHtmlHelper htmlHelper, string? filePath)
        {
            if (filePath == null) return string.Empty;
            string pattern = @"([^\/]+$)";
            var result = Regex.Match(filePath, pattern).Value;
            return !string.IsNullOrEmpty(result) ? result : filePath;
        }
    }
}
