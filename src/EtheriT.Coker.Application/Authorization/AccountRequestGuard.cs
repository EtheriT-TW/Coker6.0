using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EtheriT.Coker.Application.Authorization;

// Single-process protection. A shared atomic store is required for multiple instances.
public static class AccountRequestGuard
{
    private static readonly MemoryCache Cache = new(new MemoryCacheOptions { SizeLimit = 10000 });
    private static readonly MemoryCache RateCache = new(new MemoryCacheOptions { SizeLimit = 10000 });
    private static readonly MemoryCache FailureCache = new(new MemoryCacheOptions { SizeLimit = 10000 });
    private static readonly CacheEvictions RateEvictions = new();
    private static readonly CacheEvictions FailureEvictions = new();
    private static readonly object Sync = new();
    private const string CookieName = "Coker.CaptchaBrowser";
    private const string DiagnosticsKey = "Coker.CaptchaDiagnostics";
    private const string WouldBlockKey = "Coker.CaptchaWouldBlockReason";

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
        var captcha = operation == "captcha-image" || operation == "captcha-validate";
        // Retain the existing IP policy for account operations; CAPTCHA uses browser identity.
        var identity = captcha ? BrowserFingerprint(context) : context.Connection.RemoteIpAddress?.ToString();
        var key = $"rate:{Site(context)}:{operation}:{identity}";
        lock (Sync)
        {
            var now = DateTimeOffset.UtcNow;
            var existed = RateCache.TryGetValue(key, out Window? window);
            if (!existed || window == null || window.End <= now)
                window = new Window { End = now.AddMinutes(10) };
            if (window.Count >= limit)
            {
                Diagnose(context, operation, new { Outcome = "LimitExceeded", window.Count, Limit = limit, WindowEndUtc = window.End });
                return false;
            }
            window.Count++;
            var entriesBeforeWrite = RateCache.Count;
            RateCache.Set(key, window, CounterOptions(window.End, RateEvictions));
            var foundAfterWrite = RateCache.TryGetValue(key, out Window? saved);
            var stored = foundAfterWrite && ReferenceEquals(window, saved);
            Diagnose(context, operation, new
            {
                Outcome = stored ? "Allowed" : "CounterNotStoredAllowed",
                window.Count, Limit = limit, WindowEndUtc = window.End,
                CounterExisted = existed, FoundAfterWrite = foundAfterWrite,
                SameInstanceAfterWrite = stored, EntriesBeforeWrite = entriesBeforeWrite,
                EntriesAfterWrite = RateCache.Count, ObservedAtUtc = now
            });
            if (!stored)
                context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(AccountRequestGuard))
                    .LogWarning("Rate counter was not retained; request allowed. Operation={Operation} Site={Site} Process={Process} Count={Count} EntriesBefore={EntriesBefore} EntriesAfter={EntriesAfter} FoundAfterWrite={FoundAfterWrite} Trace={Trace}",
                        operation, Site(context), Environment.ProcessId, window.Count,
                        entriesBeforeWrite, RateCache.Count, foundAfterWrite, context.TraceIdentifier);
            // Cache capacity does not mean this visitor exceeded the limit.
            return true;
        }
    }

    public static bool Grant(string key, TimeSpan duration)
    {
        lock (Sync)
        {
            Cache.Set(key, true, new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(duration));
            return Cache.TryGetValue(key, out bool granted) && granted;
        }
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

    private static IConfiguration Configuration(HttpContext context) =>
        context.RequestServices.GetRequiredService<IConfiguration>();

    public static bool CaptchaProtectionEnabled(HttpContext context) =>
        Configuration(context).GetValue<bool>("CaptchaProtection:Enabled");

    private static bool ObserveOnly(HttpContext context) =>
        Configuration(context).GetValue<bool>("CaptchaProtection:ObserveOnly", true);

    private static bool Monitor(HttpContext context) =>
        CaptchaProtectionEnabled(context) || ObserveOnly(context);

    private static bool Enforce(HttpContext context) =>
        CaptchaProtectionEnabled(context) && !ObserveOnly(context);

    private static string ExpectedBlock(HttpContext context, string reason)
    {
        context.Items[WouldBlockKey] = reason;
        return Enforce(context) ? reason : "";
    }

    private static string Site(HttpContext context) =>
        Configuration(context).GetValue<long>("WebConfig:SiteId").ToString();

    private static string BrowserFingerprint(HttpContext context) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Browser(context))));

    private static void Diagnose(HttpContext context, string operation, object decision)
    {
        if (!context.Items.TryGetValue(DiagnosticsKey, out var value))
            context.Items[DiagnosticsKey] = value = new Dictionary<string, object>();
        ((Dictionary<string, object>)value!)[operation] = decision;
    }

    public static string Details(HttpContext context) => JsonSerializer.Serialize(new
    {
        Version = 4,
        ProtectionEnabled = CaptchaProtectionEnabled(context),
        Mode = ObserveOnly(context) ? "ObserveOnly" : CaptchaProtectionEnabled(context) ? "Enforce" : "Disabled",
        WouldBlock = context.Items.ContainsKey(WouldBlockKey),
        WouldBlockReason = context.Items.TryGetValue(WouldBlockKey, out var blockReason) ? blockReason : null,
        IdentityType = "BrowserCookie",
        BrowserFingerprint = BrowserFingerprint(context),
        Path = context.Request.Path.Value,
        ProcessId = Environment.ProcessId,
        MachineName = Environment.MachineName,
        ObservedAtUtc = DateTimeOffset.UtcNow,
        CacheSizeLimit = 10000,
        RateCacheEvictions = RateEvictions.Snapshot(),
        FailureCacheEvictions = FailureEvictions.Snapshot(),
        RateCacheEntries = RateCache.Count,
        FailureCacheEntries = FailureCache.Count,
        GrantCacheEntries = Cache.Count,
        EffectiveIp = context.Connection.RemoteIpAddress?.ToString(),
        OriginalProxyIp = context.Items["CdnTrustedProxy.OriginalRemoteIpAddress"]?.ToString(),
        Decisions = context.Items.TryGetValue(DiagnosticsKey, out var decisions) ? decisions : null
    });

    private static string FailureKey(HttpContext context) =>
        $"captcha-failure:{Site(context)}:browser:{BrowserFingerprint(context)}";

    public static bool CaptchaLocked(HttpContext context)
    {
        lock (Sync)
        {
            var locked = FailureCache.TryGetValue(FailureKey(context), out FailureWindow? state)
                && state?.LockedUntil > DateTimeOffset.UtcNow;
            Diagnose(context, "captcha-failure", new
            {
                Outcome = locked ? "Locked" : "NotLocked", Count = state?.Count ?? 0,
                Limit = 5, LockedUntilUtc = state?.LockedUntil
            });
            return locked;
        }
    }

    public static string CheckCaptchaImage(HttpContext context)
    {
        if (!Monitor(context)) return "";
        lock (Sync)
        {
            if (CaptchaLocked(context)) return ExpectedBlock(context, "CaptchaLocked");
            return Allow(context, "captcha-image", 60) ? "" : ExpectedBlock(context, "RateLimited");
        }
    }

    // Validation and failure updates are atomic within this process.
    public static string ValidateCaptcha(HttpContext context, string? id, string? code, Func<bool> validate)
    {
        var browser = Browser(context);
        var monitoring = Monitor(context);
        lock (Sync)
        {
            Consume($"verified:{browser}");
            var locked = monitoring && CaptchaLocked(context);
            var limited = monitoring && !locked && !Allow(context, "captcha-validate", 60);
            if (locked || limited)
            {
                var blocked = ExpectedBlock(context, locked ? "CaptchaLocked" : "RateLimited");
                if (blocked != "") return blocked;
            }
            var reason = string.IsNullOrWhiteSpace(id) || id.Length > 128 ||
                string.IsNullOrWhiteSpace(code) || code.Length > 16 ? "InvalidInput" :
                !Consume($"challenge:{id}") ? "ChallengeMissingExpiredOrReplayed" :
                validate() ? "" : "IncorrectCode";
            // In observation mode still validate the answer, but preserve the simulated lock.
            // Enforced requests would not reach validation while locked or rate-limited.
            if (!monitoring || locked || limited) return reason;
            var key = FailureKey(context);
            if (reason == "")
            {
                FailureCache.Remove(key);
                Diagnose(context, "captcha-failure", new { Outcome = "Cleared", Count = 0, Limit = 5 });
            }
            // Expired challenges, duplicate submits and malformed input do not lock visitors.
            else if (reason == "IncorrectCode")
            {
                var now = DateTimeOffset.UtcNow;
                if (!FailureCache.TryGetValue(key, out FailureWindow? state) || state == null || state.End <= now)
                    state = new FailureWindow { End = now.AddMinutes(15) };
                state.Count++;
                if (state.Count >= 5)
                {
                    state.End = now.AddMinutes(15);
                    state.LockedUntil = state.End;
                }
                var entriesBeforeWrite = FailureCache.Count;
                FailureCache.Set(key, state, CounterOptions(state.End, FailureEvictions));
                var foundAfterWrite = FailureCache.TryGetValue(key, out FailureWindow? saved);
                var stored = foundAfterWrite && ReferenceEquals(state, saved);
                Diagnose(context, "captcha-failure", new
                {
                    Outcome = stored ? "IncorrectCodeCounted" : "FailureCounterNotStored",
                    state.Count, Limit = 5, WindowEndUtc = state.End, LockedUntilUtc = state.LockedUntil,
                    FoundAfterWrite = foundAfterWrite, SameInstanceAfterWrite = stored,
                    EntriesBeforeWrite = entriesBeforeWrite, EntriesAfterWrite = FailureCache.Count,
                    ObservedAtUtc = now
                });
                if (stored && state.LockedUntil > now)
                {
                    var blocked = ExpectedBlock(context, "CaptchaLockedAfterFailures");
                    if (blocked != "") return blocked;
                }
            }
            return reason;
        }
    }

    private static MemoryCacheEntryOptions CounterOptions(DateTimeOffset end, CacheEvictions evictions) =>
        new MemoryCacheEntryOptions().SetSize(1).SetAbsoluteExpiration(end)
            .RegisterPostEvictionCallback(static (_, _, reason, state) =>
                ((CacheEvictions)state!).Record(reason), evictions);

    // Callbacks run asynchronously. These are cumulative process-local diagnostics,
    // not proof that a particular request caused an eviction. Never retain request data.
    private sealed class CacheEvictions
    {
        private long capacity, expired, replaced, removed, other;

        public void Record(EvictionReason reason)
        {
            switch (reason)
            {
                case EvictionReason.Capacity: Interlocked.Increment(ref capacity); break;
                case EvictionReason.Expired: Interlocked.Increment(ref expired); break;
                case EvictionReason.Replaced: Interlocked.Increment(ref replaced); break;
                case EvictionReason.Removed: Interlocked.Increment(ref removed); break;
                default: Interlocked.Increment(ref other); break;
            }
        }

        public object Snapshot() => new
        {
            Capacity = Interlocked.Read(ref capacity), Expired = Interlocked.Read(ref expired),
            Replaced = Interlocked.Read(ref replaced), Removed = Interlocked.Read(ref removed),
            Other = Interlocked.Read(ref other)
        };
    }

    private sealed class FailureWindow
    {
        internal int Count;
        internal DateTimeOffset End;
        internal DateTimeOffset? LockedUntil;
    }

    private sealed class Window
    {
        internal int Count;
        internal DateTimeOffset End;
    }
}
