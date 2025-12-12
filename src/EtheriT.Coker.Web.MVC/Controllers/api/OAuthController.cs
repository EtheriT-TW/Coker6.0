using DevExpress.CodeParser;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Shared.Dto.Authorizaion.Auth;
using EtheriT.Coker.Application.Shared.Dto.enumType.OAuth;
using EtheriT.Coker.Application.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly AuthenticationSettings _authSettings;
        private readonly StringHandler _stringHandler;
        private readonly IAccountAppService _accountAppService;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly ILogger<OAuthController> _logger;
        private readonly string DefaultDomain;

        public OAuthController(
            IOptions<AuthenticationSettings> authSettings, StringHandler StringHandler, 
            IAccountAppService accountAppService, IAuthenticationSchemeProvider schemeProvider, ILogger<OAuthController> logger)
        {
            _authSettings = authSettings.Value;
            _stringHandler = StringHandler;
            _accountAppService = accountAppService;
            _schemeProvider = schemeProvider;
            _logger = logger;
            DefaultDomain = "https://www.coker.com.tw";
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
            var TokenResult = _accountAppService.checkRedirectUrl(redirect);
            var safeRedirect = Uri.EscapeDataString(Uri.UnescapeDataString(redirect));
            if (string.IsNullOrEmpty(TokenResult.RedirectUrl)) {
                return Redirect(DefaultDomain);
            }
            if (!isValidProvider)
            {
                return Redirect($"{TokenResult.RedirectUrl}/oauth/error?code={(int)OAuthErrorTypeEnum.無效的登入方式}&&redirect={Uri.EscapeDataString(safeRedirect)}");
            }
            var properties = new AuthenticationProperties
            {
                RedirectUri = "/api/OAuth/ExternalLoginCallback",
                Items = { { "scheme", provider }, { "redirect", safeRedirect ?? "" } }
            };
            return Challenge(properties, provider);
        }
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var raw = await HttpContext.AuthenticateAsync("External");

            if (!raw.Succeeded)
            {
                _logger.LogWarning("[OAuth] External cookie not found or invalid.");
                return Redirect(DefaultDomain);
            }

            var scheme = raw.Properties?.Items.TryGetValue("scheme", out var s) == true ? s : null;
            var redirect = raw.Properties?.Items.TryGetValue("redirect", out var r) == true ? r : null;

            if (string.IsNullOrEmpty(scheme))
            {
                _logger.LogWarning($"[OAuth] 未偵測到合法登入來源，可能是直接存取或流程異常");
                return Redirect(DefaultDomain);
            }

            var result = await HttpContext.AuthenticateAsync(scheme);
            var redirectCheck = _accountAppService.checkRedirectUrl(redirect);
            var redirectBaseUrl = redirectCheck.RedirectUrl;
            if (string.IsNullOrEmpty(redirectBaseUrl)) {
                return Redirect(DefaultDomain);
            }

            var cleanRedirect = _stringHandler.RemoveQueryParam(redirect, "siteId");
            var safeRedirect = Uri.EscapeDataString(cleanRedirect);

            var user = result.Principal; // 這裡可取回第三方登入的使用者資訊
            var email = user?.FindFirst(ClaimTypes.Email)?.Value;
            var name = user?.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name)) {
                _logger.LogWarning($"[OAuth] 登入資訊不完整，請確認第三方帳號設定");
                return Redirect($"{redirectBaseUrl}/api/oauth/error?code={OAuthErrorTypeEnum.登入失敗缺少沒有信箱資料}&redirect={safeRedirect}");
            }
            var TokenResult = await _accountAppService.FrontThirdLogin(new FrontThirdLoginInputDto
            {
                Email = email,
                Name = name,
                FK_WebsiteId = redirectCheck.FK_WebsiteId
            });
            if (!TokenResult.Success) {
                return Redirect($"{redirectBaseUrl}/api/oauth/error?code={TokenResult.Error}&redirect={safeRedirect}");
            }
            await HttpContext.SignOutAsync("External");
            // ✅ 登入完成頁，可傳 token 回去給前台
            return Redirect($"{redirectBaseUrl}/api/oauth/success?token={TokenResult.Secret}&redirect={safeRedirect}");
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
