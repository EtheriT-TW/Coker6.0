using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Freight;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
