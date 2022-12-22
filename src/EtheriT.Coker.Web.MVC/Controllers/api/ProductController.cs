using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

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
        [HttpPost]
        public async Task<ResponseMessageDto> StockAddUp(ProductStockDto dto)
        {
            return await productAppService.StockAddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await productAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<ProductDto> GetProdDataOne(long Id)
        {
            return await productAppService.GetProdDataOne(Id);
        }
        [HttpGet]
        public async Task<List<ProductStockDto>> GetStockDataAll(long PId)
        {
            return await productAppService.GetStockDataAll(PId);
        }
        [HttpGet]
        public async Task<List<ProdIdTitleDto>> GetSpecType()
        {
            return await productAppService.GetSpecType();
        }
        [HttpGet]
        public async Task<List<ProdIdTitleDto>> GetSpecDetail(long typeid)
        {
            return await productAppService.GetSpecDetail(typeid);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ProdDelete(DataDelectDto dto)
        {
            return await productAppService.ProdDelete(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> StockDelete(DataDelectDto dto)
        {
            return await productAppService.StockDelete(dto);
        }
    }
}
