using EtheriT.Coker.Application.Webs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Website
{
	public interface IWebsiteApplication
	{
		public Task<List<WebsDto>> GetAll();
	}
}
