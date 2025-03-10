using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        public LoginController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        public IActionResult LoginWithLine()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/line" }, "Line");
        }

        public IActionResult LoginWithFacebook()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/facebook" }, "Facebook");
        }

        public IActionResult LoginWithGoogle()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/google" }, "Google");
        }

        public IActionResult LoginWithApple()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/apple" }, "Apple");
        }

        public IActionResult LoginWithInstagram()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/api/auth/instagram" }, "Instagram");
        }

        // 登入成功後重定向到前台並傳遞 token
        public IActionResult Callback(string returnUrl)
        {
            var token = GenerateToken(); // 假設你這裡會生成或從第三方獲得 token
            return Redirect($"{returnUrl}?token={token}");
        }

        private string GenerateToken()
        {
            // 假設這裡是生成 token 的邏輯
            return "your_generated_token";
        }
    }
}
