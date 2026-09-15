using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EtheriT.Coker.Authentication.Backoffice;

public enum BackofficeSystem
{
    Mvc,
    Platform
}

public sealed record BackofficeNavigationPreference(
    string Account,
    BackofficeSystem System,
    string Path,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Stores the last backoffice location in an encrypted, server-owned cookie shared by MVC and Platform.
/// </summary>
public sealed class BackofficeNavigationPreferenceCookie
{
    private const string CookieName = ".Coker6.Back.LastLocation";
    private const string ProtectorPurpose = "EtheriT.Coker.Backoffice.LastLocation.v1";
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(180);

    private readonly IDataProtector _protector;
    private readonly string? _cookieDomain;

    public BackofficeNavigationPreferenceCookie(
        IDataProtectionProvider dataProtectionProvider,
        IConfiguration configuration)
    {
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _cookieDomain = configuration["BackofficeAuthentication:CookieDomain"];
    }

    public BackofficeNavigationPreference? Read(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var value) ||
            string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            var preference = JsonSerializer.Deserialize<BackofficeNavigationPreference>(
                _protector.Unprotect(value));

            if (preference == null ||
                string.IsNullOrWhiteSpace(preference.Account) ||
                preference.UpdatedAt < DateTimeOffset.UtcNow.Subtract(Lifetime) ||
                !TryNormalizePath(preference.System, preference.Path, out var normalizedPath))
            {
                return null;
            }

            return preference with { Path = normalizedPath };
        }
        catch
        {
            return null;
        }
    }

    public void Write(
        HttpContext context,
        string account,
        BackofficeSystem system,
        string? path)
    {
        if (string.IsNullOrWhiteSpace(account) ||
            !TryNormalizePath(system, path, out var normalizedPath))
        {
            return;
        }

        var preference = new BackofficeNavigationPreference(
            account.Trim(),
            system,
            normalizedPath,
            DateTimeOffset.UtcNow);
        var value = _protector.Protect(JsonSerializer.Serialize(preference));

        context.Response.Cookies.Append(CookieName, value, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = "/",
            Domain = string.IsNullOrWhiteSpace(_cookieDomain) ? null : _cookieDomain,
            Expires = DateTimeOffset.UtcNow.Add(Lifetime)
        });
    }

    public static bool TryNormalizePath(
        BackofficeSystem system,
        string? path,
        out string normalizedPath)
    {
        normalizedPath = string.Empty;
        if (string.IsNullOrWhiteSpace(path))
        {
            normalizedPath = system == BackofficeSystem.Mvc ? "/Welcome" : "/";
            return true;
        }

        var value = path.Trim();
        if (value.Length > 2048 ||
            !value.StartsWith('/') ||
            value.StartsWith("//", StringComparison.Ordinal) ||
            value.Contains('\\') ||
            value.Any(char.IsControl) ||
            !Uri.TryCreate(value, UriKind.Relative, out _))
        {
            return false;
        }

        if (system == BackofficeSystem.Mvc &&
            value.StartsWith("/Account", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (system == BackofficeSystem.Platform &&
            (value.StartsWith("/api", StringComparison.OrdinalIgnoreCase) ||
             value.StartsWith("/Home", StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        normalizedPath = value;
        return true;
    }
}
