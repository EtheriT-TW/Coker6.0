using Microsoft.Extensions.Caching.Memory;

namespace EtheriT.Coker.Web.Public.Controllers.api;

// Single-process protection. A shared atomic store is required for multiple instances.
internal static class AccountRequestGuard
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions { SizeLimit = 10000 });
    private static readonly object Sync = new();
    private const string CookieName = "Coker.CaptchaBrowser";

    internal static string Browser(HttpContext context)
    {
        var browser = context.Request.Cookies[CookieName];
        if (Guid.TryParseExact(browser, "N", out _)) return browser!;
        browser = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(CookieName, browser, new CookieOptions
        {
            HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Strict,
            Path = "/", MaxAge = TimeSpan.FromMinutes(30)
        });
        return browser;
    }

    internal static bool Allow(HttpContext context, string operation, int limit)
    {
        // Do not trust caller-supplied X-Forwarded-For headers.
        var key = $"rate:{operation}:{context.Connection.RemoteIpAddress}";
        lock (Sync)
        {
            var now = DateTimeOffset.UtcNow;
            if (!Cache.TryGetValue(key, out Window? window) || window == null || window.End <= now)
                window = new Window { End = now.AddMinutes(10) };
            if (window.Count >= limit) return false;
            window.Count++;
            Cache.Set(key, window, new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(window.End));
            return Cache.TryGetValue(key, out _);
        }
    }

    internal static void Grant(string key, TimeSpan duration)
    {
        lock (Sync) Cache.Set(key, true, new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(duration));
    }

    internal static bool Consume(string key)
    {
        lock (Sync)
        {
            var granted = Cache.TryGetValue(key, out bool value) && value;
            Cache.Remove(key);
            return granted;
        }
    }

    private sealed class Window
    {
        internal int Count;
        internal DateTimeOffset End;
    }
}
