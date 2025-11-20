using EtheriT.Coker.Web.Public.Services;

namespace EtheriT.Coker.Web.Public.Middlewares
{
    public class RedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        public RedirectMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }
        public async Task InvokeAsync(HttpContext context, JumpRedirectCache cache)
        {
            var req = context.Request;
            // ------------------------------
            // 1) 主網域強制301
            // ------------------------------
            if (_env.IsProduction() && req.IsHttps) {
                await cache.EnsureDomainLoadedAsync();

                if (cache.ShouldRedirectToMainDomain(req.Host.Host))
                {
                    var main = cache.GetMainDomain();
                    var url = $"{req.Scheme}://{main}{req.Path}{req.QueryString}";

                    context.Response.Redirect(url, permanent: true);
                    return;
                }
            }

            // ------------------------------
            // 2) Route 跳頁（302）
            // ------------------------------
            await cache.ReloadRouteRulesAsync();
            var routeName = context.GetRouteData()?.Values["key"]?.ToString();
            var routeUrl = cache.EvaluateRouteRedirect(routeName);
            if (routeUrl != null)
            {
                context.Response.Redirect(routeUrl, permanent: false);
                return;
            }

            await _next(context);
        }
    }
}
