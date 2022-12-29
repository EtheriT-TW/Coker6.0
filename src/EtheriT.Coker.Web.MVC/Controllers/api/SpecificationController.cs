using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EtheriT.Coker.Application.Shared.Specification;

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
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ResponseMessageDto> AddUp([FromForm] DevExpressDto dto)
        {
            return await specificationAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await specificationAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            return await specificationAppService.Delete(Id);
        }
    }
}
