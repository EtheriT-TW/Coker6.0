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
        [HttpGet]
        public async Task<ResponseMessageDto> ActivityClick(long FK_Aid)
        {
            AdvertiseLogDto dto = new AdvertiseLogDto()
            {
                FK_Aid = FK_Aid,
                Action = (int)LogActionEnum.點擊,
            };
            return await advertiseAppService.ActivityLog(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ActivityExposure(long FK_Aid)
        {
            AdvertiseLogDto dto = new AdvertiseLogDto()
            {
                FK_Aid = FK_Aid,
                Action = (int)LogActionEnum.顯示,
            };
            return await advertiseAppService.ActivityLog(dto);
        }
    }
}
