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
        public CookieHandlingMiddleware(RequestDelegate next){
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            if (context.Request.Path.StartsWithSegments("/wwwroot") || 
                (!string.IsNullOrEmpty(context.Request.Path.Value) && context.Request.Path.Value.Contains("/upload/")))
            {
                await _next(context);
                return;
            }

            context.Response.OnStarting(() => {
                context.Response.Headers.Remove("X-Powered-By");
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-AspNet-Version");
                context.Response.Headers.Remove("X-AspNetMvc-Version");
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
                return Task.CompletedTask;
            });
            await _next(context);
        }
    }
}
