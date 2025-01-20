using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.FlowSize
{
	public interface IFlowSizeAppService
	{
		public Task<FlowSizeDto> GetMonthFlowSizes();

		public Task<JsonResult> GetFlowSizesList(DataSourceLoadOptions loadOptions);
	}
}
