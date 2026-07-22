using System.Net;

namespace EtheriT.Coker.Application.Shared.Processor
{
    /// <summary>
    /// Shared allowlist for stylesheet links rendered by public content and CSP.
    /// Keep the browser policy and server-side sanitizer on the same source list.
    /// </summary>
    public static class ExternalStylesheetPolicy
    {
        public static IReadOnlyList<string> CspSources { get; } = new[]
        {
            "*.googleapis.com",
            "*.google.com",
            "*.gstatic.com",
            "cdnjs.cloudflare.com",
            "cdn.ckeditor.com",
            "https://ecpg-stage.ecpay.com.tw",
            "https://ecpg.ecpay.com.tw",
            "logistics-stage.ecpay.com.tw",
            "logistics.ecpay.com.tw",
            "postgate-stage.ecpay.com.tw",
            "postgate.ecpay.com.tw"
        };

        public static string CspSourceExpression => string.Join(" ", CspSources);

        public static bool IsAllowedHref(string? href)
        {
            var value = WebUtility.HtmlDecode(href ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
                return false;

            // Same-origin styles must use a relative URL. Protocol-relative URLs
            // are rejected because their host still requires an explicit allowlist.
            if (value.StartsWith("/", StringComparison.Ordinal))
                return !value.StartsWith("//", StringComparison.Ordinal);

            if (value.StartsWith("./", StringComparison.Ordinal) ||
                value.StartsWith("../", StringComparison.Ordinal))
                return true;

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                !uri.IsDefaultPort)
                return false;

            return CspSources.Any(source => MatchesHost(uri.Host, source));
        }

        private static bool MatchesHost(string host, string source)
        {
            var sourceHost = source;
            if (Uri.TryCreate(source, UriKind.Absolute, out var sourceUri))
                sourceHost = sourceUri.Host;

            if (sourceHost.StartsWith("*.", StringComparison.Ordinal))
            {
                var suffix = sourceHost[2..];
                return host.Length > suffix.Length &&
                    host.EndsWith($".{suffix}", StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(host, sourceHost, StringComparison.OrdinalIgnoreCase);
        }
    }
}
