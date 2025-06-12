using EtheriT.Coker.Application.Shared.Dto.Authorizaion.Auth;
using EtheriT.Coker.Application.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly AuthenticationSettings _authSettings;
        private readonly ITokenAppService _tokenAppService;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(IOptions<AuthenticationSettings> authSettings, ITokenAppService tokenAppService, IAuthenticationSchemeProvider schemeProvider, ILogger<OAuthController> logger)
        {
            _authSettings = authSettings.Value;
            _tokenAppService = tokenAppService;
            _schemeProvider = schemeProvider;
            _logger = logger;
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
        [HttpGet]
        public async Task<IActionResult> ExternalLogin(string provider, string redirect)
        {
            var allSchemes = await _schemeProvider.GetAllSchemesAsync();
            var isValidProvider = allSchemes.Any(s => string.Equals(s.Name, provider, StringComparison.OrdinalIgnoreCase));

            if (!isValidProvider)
            {
                return BadRequest($"無效的登入方式：{provider}");
            }
            _logger.LogInformation($"[OAuth] Challenge to provider={provider}, redirect={redirect}");
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/OAuth/ExternalLoginCallback",
                Items = { { "scheme", provider }, { "redirect", redirect ?? "" } }
            };
            return Challenge(properties, provider);
        }
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var allSchemes = await _schemeProvider.GetAllSchemesAsync();
            AuthenticateResult? raw = null;
            string? scheme = null;

            foreach (var s in allSchemes)
            {
                var temp = await HttpContext.AuthenticateAsync(s.Name);
                if (temp.Succeeded && temp.Properties?.Items?.ContainsKey("scheme") == true)
                {
                    scheme = temp.Properties.Items["scheme"];
                    raw = temp;
                    break;
                }
            }

            if (string.IsNullOrEmpty(scheme) || raw == null)
                return BadRequest("未偵測到合法登入來源");

            var result = await HttpContext.AuthenticateAsync(scheme);

            if (!result.Succeeded)
                return BadRequest("登入失敗");

            var user = result.Principal; // 這裡可取回第三方登入的使用者資訊
            var email = user?.FindFirst(ClaimTypes.Email)?.Value;
            var name = user?.FindFirst(ClaimTypes.Name)?.Value;

            // ✅ 在此你可以產生 JWT 或儲存 session
            var token = "xxxxx";

            // ✅ 登入完成頁，可傳 token 回去給前台
            //return Redirect($"/oauth/success?token={token}&redirect={Uri.EscapeDataString(redirect)}");
            return new JsonResult(new
            {
                scheme,
                token,
                email,
                name
            });
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
