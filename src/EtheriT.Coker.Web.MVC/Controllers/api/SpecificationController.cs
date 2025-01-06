using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Application.Shared.Dto.Specification;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class SpecificationController : Controller
    {
        private readonly ISpecificationAppService specificationAppService;
        public SpecificationController(
            ISpecificationAppService specificationAppService
            )
        {
            this.specificationAppService = specificationAppService;
        }
        [HttpPost]
        //[ValidateAntiForgeryToken] //驗證異常暫時關閉
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ResponseMessageDto> TypeAddUp([FromForm] DevExpressDto dto)
        {
            return await specificationAppService.TypeAddUp(dto);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken] //驗證異常暫時關閉
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ResponseMessageDto> SpecAddUp_List([FromForm] DevExpressDto dto)
        {
            return await specificationAppService.SpecAddUp(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SpecAddUp_Data(SpecSpecListDto dto)
        {
            return await specificationAppService.SpecAddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllTypeList(DataSourceLoadOptions loadOptions)
        {
            return await specificationAppService.GetAllTypeList(loadOptions);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllSpecList(DataSourceLoadOptions loadOptions)
        {
            return await specificationAppService.GetAllSpecList(loadOptions);
        }
        [HttpGet]
        public async Task<List<SpecTypePickListDto>> GetPickSpecList()
        {
            return await specificationAppService.GetPickSpecList();
        }
        [HttpGet]
        public async Task<ResponseMessageDto> TypeDelete(long Id)
        {
            return await specificationAppService.TypeDelete(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> SpecDelete(long Id)
        {
            return await specificationAppService.SpecDelete(Id);
        }
    }
}
