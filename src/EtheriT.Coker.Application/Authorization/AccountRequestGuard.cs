using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;

namespace EtheriT.Coker.Application.Authorization;

// Single-process protection. A shared atomic store is required for multiple instances.
public static class AccountRequestGuard
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions { SizeLimit = 10000 });
    private static readonly object Sync = new();
    private const string CookieName = "Coker.CaptchaBrowser";

    public static string Browser(HttpContext context)
    {
        if (context.Items.TryGetValue(CookieName, out var existing)) return (string)existing!;
        var browser = context.Request.Cookies[CookieName];
        if (Guid.TryParseExact(browser, "N", out _))
        {
            context.Items[CookieName] = browser!;
            return browser!;
        }
        browser = Guid.NewGuid().ToString("N");
        context.Items[CookieName] = browser;
        context.Response.Cookies.Append(CookieName, browser, new CookieOptions
        {
            HttpOnly = true, Secure = context.Request.IsHttps, SameSite = SameSiteMode.Strict,
            Path = "/", MaxAge = TimeSpan.FromMinutes(30)
        });
        return browser;
    }

    public static bool Allow(HttpContext context, string operation, int limit)
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

    public static void Grant(string key, TimeSpan duration)
    {
        lock (Sync) Cache.Set(key, true, new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(duration));
    }

    public static bool Consume(string key)
    {
        lock (Sync)
        {
            var granted = Cache.TryGetValue(key, out bool value) && value;
            Cache.Remove(key);
            return granted;
        }
    }

    private static string[] FailureKeys(HttpContext context, string browser) =>
        new[] { $"captcha-failure:ip:{context.Connection.RemoteIpAddress}", $"captcha-failure:browser:{browser}" };

    public static bool CaptchaLocked(HttpContext context)
    {
        var browser = Browser(context);
        lock (Sync) return IsLocked(FailureKeys(context, browser));
    }

    private static bool IsLocked(string[] keys) => keys.Any(key =>
        Cache.TryGetValue(key, out FailureWindow? state) && state?.LockedUntil > DateTimeOffset.UtcNow);

    // The check, validation and counter update are atomic within this process.
    public static string ValidateCaptcha(HttpContext context, string? id, string? code, Func<bool> validate)
    {
        var browser = Browser(context);
        var keys = FailureKeys(context, browser);
        lock (Sync)
        {
            Consume($"verified:{browser}");
            if (IsLocked(keys)) return "CaptchaLocked";
            if (!Allow(context, "captcha-validate", 60)) return "RateLimited";
            var reason = string.IsNullOrWhiteSpace(id) || id.Length > 128 ||
                string.IsNullOrWhiteSpace(code) || code.Length > 16 ? "InvalidInput" :
                !Consume($"challenge:{id}") ? "ChallengeMissingExpiredOrReplayed" :
                validate() ? "" : "IncorrectCode";
            if (reason == "")
            {
                foreach (var key in keys) Cache.Remove(key);
                return reason;
            }
            var now = DateTimeOffset.UtcNow;
            foreach (var key in keys)
            {
                if (!Cache.TryGetValue(key, out FailureWindow? state) || state == null)
                    state = new FailureWindow { End = now.AddMinutes(15) };
                state.Count++;
                if (state.Count >= 5) state.End = state.LockedUntil = now.AddMinutes(15);
                Cache.Set(key, state, new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(state.End));
            }
            return IsLocked(keys) ? "CaptchaLockedAfterFailures" : reason;
        }
    }

    private sealed class FailureWindow
    {
        internal int Count;
        internal DateTimeOffset End;
        internal DateTimeOffset LockedUntil;
    }

    private sealed class Window
    {
        internal int Count;
        internal DateTimeOffset End;
    }
}
