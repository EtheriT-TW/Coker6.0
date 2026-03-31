using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Freight;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EtheriT.Coker.Application.Common;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class FreightController : Controller
    {
        private readonly IFreightAppService freightAppService;
        public FreightController(
            IFreightAppService freightAppService
            )
        {
            this.freightAppService = freightAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(FreightDto dto)
        {
            return await freightAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await freightAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<JsonResult> GetLogisticsBoxAllList(DataSourceLoadOptions loadOptions)
        {
            return await freightAppService.GetLogisticsBoxAllList(loadOptions);
        }
        [HttpPost]
        public async Task<IActionResult> LogisticsBoxAddUp(GetLogisticsBoxAllListInputDto dto)
        {
            var result = await freightAppService.LogisticsBoxAddUp(dto);
            return result.ToActionResult(this);
        }
        [HttpGet]
        public async Task<IActionResult> LogisticsBoxGetOne(long Id)
        {
            var result = await freightAppService.LogisticsBoxGetOne(Id);
            return result.ToActionResult(this);
        }
        [HttpDelete]
        public async Task<IActionResult> LogisticsBoxDelete(long Id)
        {
            var result = await freightAppService.LogisticsBoxDelete(Id);
            return result.ToActionResult(this);
        }
        [HttpGet]
        public async Task<FreightDto> GetOne(long Id)
        {
            return await freightAppService.GetOne(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            return await freightAppService.Delete(Id);
        }
    }
}
