using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Authorization
{
	public interface ICaptchaAppService
	{
		public MemoryStream Captcha(string id);
		public ResponseMessageDto Validate(string id, string code);
	}
}
