using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.FlowSize;
using EtheriT.Coker.Web.MVC.Models.Dacshboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class FlowSizeController : Controller
	{
		private readonly LoginUserData loginUserData;
		private readonly IFlowSizeAppService flowSizeAppService;
		private readonly IConfiguration configuration;
		public FlowSizeController(LoginUserData loginUserData, IFlowSizeAppService flowSizeAppService, IConfiguration configuration)
		{
			this.loginUserData = loginUserData;
			this.flowSizeAppService = flowSizeAppService;
			this.configuration = configuration;
		}
		
		[HttpGet]
		public async Task<JsonResult> FlowSizes(string type, DataSourceLoadOptions loadOptions)
		{
			return await flowSizeAppService.GetFlowSizesList(type, loadOptions);
		}
	}
}
