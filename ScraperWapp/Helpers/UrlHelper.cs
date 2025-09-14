using System.Xml.Linq;

namespace ScraperWapp.Helpers
{
    public static class CustomUrlHelper
    {
        public static string BuildGetUrl(string baseUrl, IDictionary<string, string> inputs)
        {
            if (!baseUrl.EndsWith("?") && !baseUrl.Contains("?"))
                baseUrl += "?";

            var query = string.Join("&",
                inputs
                    .Where(kv => !string.IsNullOrEmpty(kv.Key))
                    .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value ?? "")}")
            );

            return baseUrl + query;
        }
    } 
}

