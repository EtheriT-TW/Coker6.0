using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using DevExtreme.AspNet.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class AdvertiseController : Controller
    {
        private readonly IAdvertiseAppService advertiseAppService;

        public AdvertiseController(IAdvertiseAppService advertiseAppService)
        {
            this.advertiseAppService = advertiseAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(AdvertiseDto dto)
        {
            return await advertiseAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetEnterAdList(DataSourceLoadOptions loadOptions)
        {
            return await advertiseAppService.GetList(loadOptions, 1);
        }
        [HttpGet]
        public async Task<JsonResult> GetRightSideAdList(DataSourceLoadOptions loadOptions)
        {
            return await advertiseAppService.GetList(loadOptions, 2);
        }
        [HttpGet]
        public async Task<AdvertiseGetDataDto> GetDataOne(long Id)
        {
            return await advertiseAppService.GetDataOne(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            return await advertiseAppService.Delete(Id);
        }
    }
}
