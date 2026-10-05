using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Mvc;
using SimpleCaptcha;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CaptchaController : Controller
    {
		private readonly ICaptchaAppService captchaAppService;
		public CaptchaController(ICaptchaAppService captchaAppService)
        {
            this.captchaAppService = captchaAppService;
        }
        public IActionResult Index(string id)
        {
            Response.Headers.CacheControl = "no-store";
            if (string.IsNullOrWhiteSpace(id) || id.Length > 128) return BadRequest();
            if (AccountRequestGuard.CaptchaLocked(HttpContext)) return StatusCode(429);
            if (!AccountRequestGuard.Allow(HttpContext, "captcha-image", 60)) return StatusCode(429);
            AccountRequestGuard.Grant($"challenge:{id}", TimeSpan.FromMinutes(5));
            return File(captchaAppService.Captcha(id), "image/png");
        }

        public async Task<ResponseMessageDto> Validate(string id, string code)
        {
            Response.Headers.CacheControl = "no-store";
            var browser = AccountRequestGuard.Browser(HttpContext);
            var result = await captchaAppService.ValidateAsync(id, code, "CaptchaApi");
            if (result.Success)
                AccountRequestGuard.Grant($"verified:{browser}", TimeSpan.FromMinutes(2));
            return result;

		}
    }
}
