using EtheriT.Coker.Authentication.Backoffice;

namespace EtheriT.Coker.Web.MVC.Middleware;

public sealed class BackofficeNavigationPreferenceMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        BackofficeNavigationPreferenceCookie preferenceCookie)
    {
        if (HttpMethods.IsGet(context.Request.Method) &&
            context.User.Identity?.IsAuthenticated == true &&
            !context.Request.Path.StartsWithSegments("/api") &&
            !context.Request.Path.StartsWithSegments("/Account"))
        {
            var account = context.User.Identity.Name;
            var path = $"{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";
            preferenceCookie.Write(context, account ?? string.Empty, BackofficeSystem.Mvc, path);
        }

        await next(context);
    }
}
