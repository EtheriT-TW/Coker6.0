using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace EtheriT.Coker.Authentication.Backoffice;

public static class BackofficeAuthenticationExtensions
{
    public static AuthenticationBuilder AddCokerBackofficeAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableJwtBearer = true,
        Action<CookieAuthenticationOptions>? configureCookie = null)
    {
        var dataProtection = services
            .AddDataProtection()
            .SetApplicationName(BackofficeAuthenticationDefaults.DataProtectionApplicationName);

        services.Configure<BackofficeSessionOptions>(
            configuration.GetSection(BackofficeSessionOptions.SectionName));
        services.AddSingleton<BackofficeNavigationPreferenceCookie>();

        var keyPath = configuration.GetValue<string>("BackofficeAuthentication:DataProtectionKeysPath");
        if (!string.IsNullOrWhiteSpace(keyPath))
        {
            dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keyPath));
        }

        var authentication = services.AddAuthentication(options =>
        {
            var defaultScheme = enableJwtBearer
                ? BackofficeAuthenticationDefaults.PolicyScheme
                : CookieAuthenticationDefaults.AuthenticationScheme;

            options.DefaultScheme = defaultScheme;
            options.DefaultAuthenticateScheme = defaultScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });

        authentication.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/";
            options.ExpireTimeSpan = TimeSpan.FromDays(1);
            options.SlidingExpiration = true;
            options.Cookie.Name = BackofficeAuthenticationDefaults.CookieName;

            var cookieDomain = configuration.GetValue<string>("BackofficeAuthentication:CookieDomain");
            if (!string.IsNullOrWhiteSpace(cookieDomain))
            {
                options.Cookie.Domain = cookieDomain;
            }

            configureCookie?.Invoke(options);

            var configuredValidatePrincipal = options.Events.OnValidatePrincipal;
            options.Events.OnValidatePrincipal = async context =>
            {
                if (configuredValidatePrincipal != null)
                {
                    await configuredValidatePrincipal(context);
                }

                if (context.Principal?.Identity?.IsAuthenticated != true)
                {
                    return;
                }

                var sessionValidator = context.HttpContext.RequestServices
                    .GetService<IBackofficeSessionValidator>();

                // Hosts that have not registered a persistent backoffice session store
                // continue to use the shared cookie without database session validation.
                if (sessionValidator == null)
                {
                    return;
                }

                var account = context.Principal.Identity.Name;
                var sessionValue = context.Principal.FindFirstValue(ClaimTypes.Sid);

                if (string.IsNullOrWhiteSpace(account) ||
                    !Guid.TryParse(sessionValue, out var sessionId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var validation = await sessionValidator.ValidateAndRenewAsync(
                    account,
                    sessionId,
                    context.HttpContext.RequestAborted);

                if (!validation.IsValid)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                if (validation.WasRenewed)
                {
                    context.ShouldRenew = true;
                }
            };
        });

        if (!enableJwtBearer)
        {
            return authentication;
        }

        var signKey = configuration.GetValue<string>("JwtSettings:SignKey")
            ?? throw new InvalidOperationException("JwtSettings:SignKey 尚未設定");

        authentication
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.IncludeErrorDetails = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                    ValidateIssuer = true,
                    ValidIssuer = configuration.GetValue<string>("JwtSettings:Issuer"),
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT 驗證失敗: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"JWT 驗證成功: {context.Principal?.Identity?.Name}");
                        return Task.CompletedTask;
                    }
                };
            })
            .AddPolicyScheme(
                BackofficeAuthenticationDefaults.PolicyScheme,
                "Select JWT or Cookie dynamically",
                options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authorization = context.Request.Headers[HeaderNames.Authorization].ToString();
                        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                            ? JwtBearerDefaults.AuthenticationScheme
                            : CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                });

        return authentication;
    }
}
