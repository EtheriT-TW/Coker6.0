using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Webs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
	public interface IWebsiteApplication
	{
		public Task<List<WebsDto>> GetAll();
		public Task<ResponseMessageDto> Exchange(WebExchangeDto dto);
	}
}
