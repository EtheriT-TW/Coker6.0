using Microsoft.AspNetCore.Mvc;
using SimpleCaptcha;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CaptchaController : Controller
    {
        private readonly ICaptcha _captcha;
        public CaptchaController(ICaptcha captcha)
        {
            _captcha = captcha;
        }
        public IActionResult Index(string id)
        {
            var info = _captcha.Generate(id);
            var stream = new MemoryStream(info.CaptchaByteData);
            return File(stream, "image/png");
        }

        public IActionResult Validate(string id, string code)
        {
            var result = _captcha.Validate(id, code);
            return Json(new { success = result });
        }
    }
}
