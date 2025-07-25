using DevExpress.CodeParser;
using DevExpress.XtraPrinting.Native;
using EtheriT.Coker.Application;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.Public.Middlewares
{
    public class CookieHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        public CookieHandlingMiddleware(
            RequestDelegate next,
            IConfiguration configuration
        ){
            _next = next;
            _configuration = configuration;
        }
        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            if (context.Request.Path.StartsWithSegments("/wwwroot") || 
                (!string.IsNullOrEmpty(context.Request.Path.Value) && context.Request.Path.Value.Contains("/upload/")))
            {
                await _next(context);
                return;
            }
            long siteId = _configuration.GetValue<long>("WebConfig:SiteId");
            string orgName = await GetOrgNameAsync(context, siteId);
            context.Response.OnStarting(() => {
                context.Response.Headers.Remove("X-Powered-By");
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-AspNet-Version");
                context.Response.Headers.Remove("X-AspNetMvc-Version");
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
                if (context.Request.Cookies.ContainsKey("Token"))
                {
                    string Token = context.Request.Cookies["Token"] ?? "";
                    string RefreshToken = context.Request.Cookies["RefreshToken"] ?? "";
                    string Remember = context.Request.Cookies["Remember"] ?? "";
                    CookieOptions tokenOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                    };
                    CookieOptions refreshTokenOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddMonths(3)
                    };
                    if (context.Request.Path.StartsWithSegments($"/{orgName}/ShoppingCar"))
                    {
                        tokenOptions.SameSite = SameSiteMode.None;
                        refreshTokenOptions.SameSite = SameSiteMode.None;
                    }
                    if (!context.Response.Headers.ContainsKey("Set-Cookie") ||
                        !context.Response.Headers["Set-Cookie"].Any(header => !string.IsNullOrEmpty(header) && header.Contains("Token=")))
                    {
                        if (string.IsNullOrEmpty(Remember) || Remember == "1") {
                            context.Response.Cookies.Append("Token", Token, tokenOptions);
                            context.Response.Cookies.Append("RefreshToken", RefreshToken, refreshTokenOptions);
                            context.Response.Cookies.Append("Remember", Remember, refreshTokenOptions);
                        }
                    }
                }
                return Task.CompletedTask;
            });
            await _next(context);
        }
        private async Task<string> GetOrgNameAsync(HttpContext context, long siteId)
        {
            using (var scope = context.RequestServices.CreateScope())
            {
                var websiteApplication = scope.ServiceProvider.GetRequiredService<IWebsiteApplication>();
                return await websiteApplication.GetOrgName(siteId);
            }
        }
    }
}
