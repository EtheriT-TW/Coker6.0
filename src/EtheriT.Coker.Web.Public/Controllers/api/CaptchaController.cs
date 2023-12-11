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
            return File(captchaAppService.Captcha(id), "image/png");
        }

        public ResponseMessageDto Validate(string id, string code)
        {
            return captchaAppService.Validate(id, code);

		}
    }
}
