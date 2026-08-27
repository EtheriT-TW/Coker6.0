using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EtheriT.Coker.Web.Public.Authentication
{
    public sealed class FrontJwtBearerEvents : JwtBearerEvents
    {
        private readonly IConfiguration configuration;
        private readonly IFrontSessionValidator sessionValidator;

        public FrontJwtBearerEvents(
            IConfiguration configuration,
            IFrontSessionValidator sessionValidator)
        {
            this.configuration = configuration;
            this.sessionValidator = sessionValidator;
        }

        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var websiteId = configuration.GetValue<long>("WebConfig:SiteId");
            var validation = await sessionValidator.ValidateAsync(
                context.Principal,
                websiteId,
                context.HttpContext.RequestAborted);

            if (!validation.IsValid)
                context.Fail(validation.Error);
        }
    }
}
