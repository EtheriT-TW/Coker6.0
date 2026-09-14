using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace EtheriT.Coker.Authentication.Backoffice;

public static class BackofficeAuthenticationExtensions
{
    public static AuthenticationBuilder AddCokerBackofficeAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var signKey = configuration.GetValue<string>("JwtSettings:SignKey")
            ?? throw new InvalidOperationException("JwtSettings:SignKey 尚未設定");

        return services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = BackofficeAuthenticationDefaults.PolicyScheme;
                options.DefaultAuthenticateScheme = BackofficeAuthenticationDefaults.PolicyScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.Cookie.Name = BackofficeAuthenticationDefaults.CookieName;
            })
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
    }
}
