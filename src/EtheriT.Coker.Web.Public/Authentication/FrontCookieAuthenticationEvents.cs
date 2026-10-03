using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Authentication;

namespace EtheriT.Coker.Web.Public.Authentication
{
    public sealed class FrontCookieAuthenticationEvents : CookieAuthenticationEvents
    {
        private readonly IConfiguration configuration;
        private readonly IFrontSessionValidator sessionValidator;

        public FrontCookieAuthenticationEvents(
            IConfiguration configuration,
            IFrontSessionValidator sessionValidator)
        {
            this.configuration = configuration;
            this.sessionValidator = sessionValidator;
        }

        public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var validation = await sessionValidator.ValidateAsync(
                context.Principal,
                websiteId,
                context.HttpContext.RequestAborted);

            if (!validation.IsValid)
                context.RejectPrincipal();
        }

        public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
        {
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
            return base.RedirectToLogin(context);
        }

        public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
        {
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
            return base.RedirectToAccessDenied(context);
        }
    }
}
