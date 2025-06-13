using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ITokenAppService tokenAppService;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        public AuthController(ITokenAppService tokenAppService, IAuthenticationSchemeProvider schemeProvider)
        {
            this.tokenAppService = tokenAppService;
            _schemeProvider = schemeProvider;
        }
        public async Task<ResponseMessageDto> Line()
        {
            ResponseMessageDto output = new ResponseMessageDto();
            try {
                var authenticateResult = await HttpContext.AuthenticateAsync();
                var claims = authenticateResult.Principal.Claims;
                if (claims != null) {
                    var lineId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                    var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                }throw new Exception("No claims found");
            }
            catch {
                output.Message = "登入失敗";
            }
            return output;
        }
        public async Task<IActionResult> ExternalLogin(string provider, string redirect)
        {
            var allSchemes = await _schemeProvider.GetAllSchemesAsync();
            var isValidProvider = allSchemes.Any(s => string.Equals(s.Name, provider, StringComparison.OrdinalIgnoreCase));

            if (!isValidProvider)
            {
                return BadRequest($"無效的登入方式：{provider}");
            }

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth", new { redirect });

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                Items = { { "scheme", provider }, { "redirect", redirect ?? "" } }
            };

            return Challenge(properties, provider);
        }

        public async Task<IActionResult> ExternalLoginCallback(string redirect)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!result.Succeeded)
                return BadRequest("登入失敗");

            var user = result.Principal; // 這裡可取回第三方登入的使用者資訊
            var email = user?.FindFirst(ClaimTypes.Email)?.Value;
            var name = user?.FindFirst(ClaimTypes.Name)?.Value;

            // ✅ 在此你可以產生 JWT 或儲存 session
            var token = "xxxxx";

            // ✅ 登入完成頁，可傳 token 回去給前台
            return Redirect($"/oauth/success?token={token}&redirect={Uri.EscapeDataString(redirect)}");
        }
    }
}
