using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductAppService productAppService;
        public ProductController(
            IProductAppService productAppService
            )
        {
            this.productAppService = productAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ProductAddUp(ProductDto dto)
        {
            return await productAppService.ProductAddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await productAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<ProductDto> ProdGetOne(long Id)
        {
            return await productAppService.ProdGetOne(Id);
        }
        [HttpGet]
        public async Task<List<ProductStockDto>> ProdStockGet(long PId)
        {
            return await productAppService.ProdStockGet(PId);
        }
        [HttpGet]
        public async Task<List<ProdIdTitleDto>> GetSpecType(long webid)
        {
            return await productAppService.GetSpecType(webid);
        }
        [HttpGet]
        public async Task<List<ProdIdTitleDto>> GetSpecDetail(long typeid)
        {
            return await productAppService.GetSpecDetail(typeid);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ProdDelete(long Id)
        {
            return await productAppService.ProdDelete(Id);
        }
    }
}
