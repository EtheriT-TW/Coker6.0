using EtheriT.Coker.Application;

namespace EtheriT.Coker.Web.Public.Services
{
    public class JumpRedirectCache
    {
        private Dictionary<string, string> _rules = new(StringComparer.OrdinalIgnoreCase);
        private string? _mainDomain;
        private readonly object _lock = new();
        private readonly IServiceScopeFactory _scopeFactory;
        public JumpRedirectCache(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        // ----------------------------------------------------
        //  DOMAIN：只載一次主網域
        // ----------------------------------------------------
        public async Task EnsureDomainLoadedAsync()
        {

            using var scope = _scopeFactory.CreateScope();
            var websiteApp = scope.ServiceProvider.GetRequiredService<IWebsiteApplication>();
            var defaultUrl = await websiteApp.GetDomain();

            var host = NormalizeDomainHost(defaultUrl);

            lock (_lock)
            {
                _mainDomain = host;
            }
        }
        public string? GetMainDomain()
        {
            lock (_lock)
            {
                return _mainDomain;
            }
        }
        public bool ShouldRedirectToMainDomain(string currentHost)
        {
            string? main;
            lock (_lock)
            {
                main = _mainDomain;
            }

            if (string.IsNullOrWhiteSpace(main))
                return false;

            return !string.Equals(currentHost, main, StringComparison.OrdinalIgnoreCase);
        }

        private static string? NormalizeDomainHost(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return uri.Host;

            // fallback: 資料庫若存 "www.xxx.com"
            return url
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Trim()
                .TrimEnd('/');
        }
        // ----------------------------------------------------
        //  ROUTE：跳頁規則
        // ----------------------------------------------------
        public bool TryGetRedirect(string routeName, out string url)
        {
            var snapshot = _rules;
            return snapshot.TryGetValue(routeName, out url!);
        }
        public string? EvaluateRouteRedirect(string? routeName)
        {
            if (string.IsNullOrWhiteSpace(routeName))
                return null;

            if (!TryGetRedirect(routeName, out var target))
                return null;

            if (!TryNormalizeTargetUrl(target, out var safeUrl))
                return null;

            return safeUrl;
        }

        public async Task ReloadRouteRulesAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var app = scope.ServiceProvider.GetRequiredService<IWebMenuApplication>();

            var rules = await app.GetJumpRulesAsync();

            var dict = rules.ToDictionary(
                x => x.RouteName,
                x => x.TargetUrl,
                StringComparer.OrdinalIgnoreCase
            );

            lock (_lock)
            {
                _rules = dict;
            }
        }
        public bool TryNormalizeTargetUrl(string targetUrl, out string safeUrl)
        {
            safeUrl = null;

            if (string.IsNullOrWhiteSpace(targetUrl))
                return false;

            targetUrl = targetUrl.Trim();

            // 1. Absolute URL
            if (Uri.TryCreate(targetUrl, UriKind.Absolute, out var absUri))
            {
                if (absUri.Scheme == Uri.UriSchemeHttp || absUri.Scheme == Uri.UriSchemeHttps)
                {
                    safeUrl = absUri.ToString();
                    return true;
                }
                return false;
            }

            // 2. Site-relative path
            if (targetUrl.StartsWith("/"))
            {
                safeUrl = targetUrl;
                return true;
            }

            return false;
        }
    }
}
