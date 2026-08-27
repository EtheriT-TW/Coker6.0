using Microsoft.AspNetCore.Authentication.Cookies;

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
    }
}
