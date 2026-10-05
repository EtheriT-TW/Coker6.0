using EtheriT.Coker.Application.Dto;
using SimpleCaptcha;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace EtheriT.Coker.Application.Authorization
{
	public class CaptchaAppService : ICaptchaAppService
	{
		private readonly ICaptcha _captcha;
		private readonly IHttpContextAccessor accessor;
		private readonly FrontAccountEventRecorder events;
		private readonly IConfiguration configuration;
		public CaptchaAppService(ICaptcha captcha, IHttpContextAccessor accessor,
            FrontAccountEventRecorder events, IConfiguration configuration) {
			_captcha = captcha;
            this.accessor = accessor;
            this.events = events;
            this.configuration = configuration;
		}
		public MemoryStream Captcha(string id)
		{
			var info = _captcha.Generate(id);
			var stream = new MemoryStream(info.CaptchaByteData);
			return stream;
		}

		public async Task<ResponseMessageDto> ValidateAsync(string? id, string? code, string source)
		{
            var context = accessor.HttpContext;
            var reason = context == null ? "RequestContextMissing" :
                AccountRequestGuard.ValidateCaptcha(context, id, code, () => _captcha.Validate(id!, code!));
            var success = reason == "";
            await events.RecordAsync(new Account_Log
            {
                WebsiteId = configuration.GetValue<long>("WebConfig:SiteId"),
                Status = (int)AccountStatusEnum.圖形驗證, EventName = "CaptchaValidation",
                VerificationMethod = source, Success = success, FailureReason = success ? null : reason
            });
            return new ResponseMessageDto
            {
                Success = success,
                Error = success ? null : reason.StartsWith("CaptchaLocked")
                    ? "驗證失敗次數過多，請於 15 分鐘後再試"
                    : reason == "RateLimited" ? "操作過於頻繁，請稍後再試" : "驗證碼錯誤或已失效，請重新取得驗證碼"
            };
		}
	}
}
