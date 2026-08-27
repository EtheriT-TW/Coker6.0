using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace EtheriT.Coker.Web.Public.Authentication
{
    public static class AuthenticationServiceExtensions
    {
        private const string FrontAuthenticationScheme = "JWT_OR_COOKIE";

        public static IServiceCollection AddFrontAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IFrontSessionValidator, FrontSessionValidator>();
            services.AddScoped<FrontCookieAuthenticationEvents>();
            services.AddScoped<FrontJwtBearerEvents>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = FrontAuthenticationScheme;
                    options.DefaultChallengeScheme = FrontAuthenticationScheme;
                    options.DefaultAuthenticateScheme = FrontAuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/";
                    options.ExpireTimeSpan = TimeSpan.FromDays(1);
                    options.Cookie.Name = $".Coker6.Front.Auth.{configuration.GetValue<long>("WebConfig:SiteId")}";
                    options.EventsType = typeof(FrontCookieAuthenticationEvents);
                })
                .AddJwtBearer(options =>
                {
                    var signKey = configuration.GetValue<string>("JwtSettings:SignKey")
                        ?? throw new InvalidOperationException("JwtSettings:SignKey 尚未設定");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration.GetValue<string>("JwtSettings:Issuer"),
                        ValidAudience = configuration.GetValue<string>("JwtSettings:Audience"),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signKey)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                    options.EventsType = typeof(FrontJwtBearerEvents);
                })
                .AddPolicyScheme(FrontAuthenticationScheme, FrontAuthenticationScheme, options =>
                {
                    options.ForwardDefaultSelector = context =>
                    {
                        var authorization = context.Request.Headers[HeaderNames.Authorization].ToString();
                        return authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                            ? JwtBearerDefaults.AuthenticationScheme
                            : CookieAuthenticationDefaults.AuthenticationScheme;
                    };
                });

            return services;
        }
    }
}
