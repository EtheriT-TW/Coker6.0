using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AdvertiseController : Controller
    {

        private readonly IAdvertiseAppService advertiseAppService;
        public AdvertiseController(IAdvertiseAppService advertiseAppService)
        {
            this.advertiseAppService = advertiseAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ActivityClick(AdvertiseLogDto dto)
        {
            dto.Action = (int)LogActionEnum.點擊;
            return await advertiseAppService.ActivityLog(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ActivityExposure(AdvertiseLogDto dto)
        {
            dto.Action = (int)LogActionEnum.顯示;
            return await advertiseAppService.ActivityLog(dto);
        }
    }
}
