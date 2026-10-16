using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Mvc;
using SimpleCaptcha;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CaptchaController : Controller
    {
		private readonly ICaptchaAppService captchaAppService;
        private readonly FrontAccountEventRecorder events;
        private readonly IConfiguration configuration;
		public CaptchaController(ICaptchaAppService captchaAppService,
            FrontAccountEventRecorder events, IConfiguration configuration)
        {
            this.captchaAppService = captchaAppService;
            this.events = events;
            this.configuration = configuration;
        }
        public async Task<IActionResult> Index(string? id)
        {
            Response.Headers.CacheControl = "no-store";
            if (string.IsNullOrWhiteSpace(id) || id.Length > 128)
            {
                await RecordImageAsync("InvalidInput");
                return BadRequest();
            }
            var reason = AccountRequestGuard.CheckCaptchaImage(HttpContext);
            if (reason != "")
            {
                await RecordImageAsync(reason);
                return StatusCode(429);
            }
            MemoryStream image;
            try
            {
                image = captchaAppService.Captcha(id);
            }
            catch (Exception)
            {
                await RecordImageAsync("ImageGenerationFailed");
                throw;
            }
            if (!AccountRequestGuard.Grant($"challenge:{id}", TimeSpan.FromMinutes(5)))
            {
                image.Dispose();
                await RecordImageAsync("ChallengeStoreUnavailable");
                return StatusCode(503);
            }
            await RecordImageAsync(null);
            return File(image, "image/png");
        }

        private Task RecordImageAsync(string? reason) => events.RecordAsync(new Account_Log
        {
            WebsiteId = configuration.GetValue<long>("WebConfig:SiteId"),
            Status = (int)AccountStatusEnum.圖形驗證,
            EventName = "CaptchaImage", VerificationMethod = "CaptchaImage",
            Success = reason == null, FailureReason = reason,
            DetailsJson = AccountRequestGuard.Details(HttpContext)
        });

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
