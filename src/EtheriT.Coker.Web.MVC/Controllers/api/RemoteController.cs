using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using EtheriT.Coker.Application.Shared.Remote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	[Authorize]
	public class RemoteController : Controller
	{
		private readonly IRemoteAppService remoteAppService;
		public RemoteController(IRemoteAppService remoteAppService) {
			this.remoteAppService = remoteAppService;
		}
		[HttpGet]
		public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
		{
			return await remoteAppService.GetAllList(loadOptions);
		}
		[HttpGet]
		public async Task<JsonResult> GetPageList(DataSourceLoadOptions loadOptions)
		{
			return await remoteAppService.GetPageList(loadOptions);
		}
        [HttpPost]
        public async Task<ResponseMessageDto> GetRemoteCount(GetRemoteCountInputDto dto)
        {
            return await remoteAppService.GetRemoteCount(dto);
        }
    }
}
