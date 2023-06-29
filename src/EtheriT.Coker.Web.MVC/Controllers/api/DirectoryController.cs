using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class DirectoryController : Controller
    {
        private readonly IDirectoryAppService directoryAppService;

        public DirectoryController(IDirectoryAppService directoryAppService)
        {
            this.directoryAppService = directoryAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(DirectoryAddUpDto dto)
        {
            return await directoryAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<DirectoryGetDataDto> GetDataOne(long Id)
        {
            return await directoryAppService.GetDataOne(Id);
        }
        [HttpPost]
        public async Task<MenuItemDto> GetReleMenu(DataIdWebsiteIdDto dto)
        {
            return await directoryAppService.GetReleMenu(dto);
        }
        [HttpPost]
        public async Task<DirectoryReleInfoGetDto> GetReleInfo(DirectoryReleInfoInputDto dto)
        {
            return await directoryAppService.GetReleInfo(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await directoryAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            return await directoryAppService.Delete(Id);
        }
    }
}
