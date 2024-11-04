using Microsoft.AspNetCore.Antiforgery;
using System.Diagnostics;

namespace EtheriT.Coker.Web.MVC.Middleware
{
    public class AntiforgeryDebugMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAntiforgery _antiforgery;

        public AntiforgeryDebugMiddleware(RequestDelegate next, IAntiforgery antiforgery)
        {
            _next = next;
            _antiforgery = antiforgery;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var tokens = _antiforgery.GetAndStoreTokens(context);

            // 在這裡設定斷點，檢查 tokens.RequestToken 和 tokens.HeaderName
            Debug.WriteLine($"Request Token: {tokens.RequestToken}");
            Debug.WriteLine($"Header Name: {tokens.HeaderName}");

            await _next(context);
        }
    }
}
