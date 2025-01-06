using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DirectoryController : Controller
    {

        private readonly IDirectoryAppService directoryAppService;
        public DirectoryController(IDirectoryAppService directoryAppService)
        {
            this.directoryAppService = directoryAppService;
        }

        [HttpPost]
        public async Task<DirectoryReleInfoGetDto> GetReleInfo(DirectoryReleInfoInputDto dto)
        {
            return await directoryAppService.GetReleInfo(dto);
        }
        [HttpPost]
        public async Task<MenuItemDto> GetReleMenu(DataIdWebsiteIdDto dto)
        {
            return await directoryAppService.GetReleMenu(dto);
        }
        [HttpPost]
        public async Task<List<AdvertiseDisplayDto>> GetReleAd(DataIdWebsiteIdDto dto)
        {
            return await directoryAppService.GetReleAd(dto);
        }

        [HttpPost]
        public async Task<List<KeyValueDto>> SwitchPage(DirectorySwitchPageDto dto)
        {
            return await directoryAppService.SwitchPage(dto);
        }
    }
}
