using EtheriT.Coker.Application.Dto;
using SimpleCaptcha;

namespace EtheriT.Coker.Application.Authorization
{
	public class CaptchaAppService : ICaptchaAppService
	{
		private readonly ICaptcha _captcha;
		public CaptchaAppService(ICaptcha captcha) {
			_captcha = captcha;
		}
		public MemoryStream Captcha(string id)
		{
			var info = _captcha.Generate(id);
			var stream = new MemoryStream(info.CaptchaByteData);
			return stream;
		}

		public ResponseMessageDto Validate(string id,string code)
		{
			var result = _captcha.Validate(id, code);
			return new ResponseMessageDto { Success = result };
		}
	}
}
