using EtheriT.Coker.Application.Shared.Dto.Authorizaion.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly AuthenticationSettings _authSettings;

        public OAuthController(IOptions<AuthenticationSettings> authSettings)
        {
            _authSettings = authSettings.Value;
        }
        [HttpGet]
        public IActionResult GetEnabledProviders()
        {
            var result = new LoginOptionsDto
            {
                LineEnabled = IsValidLine(_authSettings.Line),
                GoogleEnabled = IsValidGoogle(_authSettings.Google),
                FacebookEnabled = IsValidFacebook(_authSettings.Facebook),
                AppleEnabled = IsValidApple(_authSettings.Apple)
            };

            return Ok(result);
        }

        private bool IsValidLine(LineConfig config)
        {
            return !string.IsNullOrWhiteSpace(config?.ChannelId) &&
                   !string.IsNullOrWhiteSpace(config?.ChannelSecret);
        }

        private bool IsValidGoogle(ProviderConfig config)
        {
            return !string.IsNullOrWhiteSpace(config?.ClientId) &&
                   !string.IsNullOrWhiteSpace(config?.ClientSecret);
        }

        private bool IsValidFacebook(FacebookConfig config)
        {
            return !string.IsNullOrWhiteSpace(config?.AppId) &&
                   !string.IsNullOrWhiteSpace(config?.AppSecret);
        }

        private bool IsValidApple(AppleConfig config)
        {
            return !string.IsNullOrWhiteSpace(config?.ClientId) &&
                   !string.IsNullOrWhiteSpace(config?.TeamId) &&
                   !string.IsNullOrWhiteSpace(config?.KeyId) &&
                   !string.IsNullOrWhiteSpace(config?.PrivateKeyPath);
        }
    }
}
