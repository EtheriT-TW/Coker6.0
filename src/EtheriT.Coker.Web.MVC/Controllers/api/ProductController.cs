using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
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
        public async Task<ResponseMessageDto> StockAddUp(List<ProductStockDto> dto)
        {
            return await productAppService.StockAddUp(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> TechCertAddUp(List<ProductTechCertDto> dto)
        {
            return await productAppService.TechCertAddUp(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ProdPriceAddUp(List<ProductPriceDto> dto)
        {
            return await productAppService.ProdPriceAddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await productAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllSpecList(DataSourceLoadOptions loadOptions)
        {
            return await productAppService.GetAllSpecList(loadOptions);
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
        public async Task<List<TechnicalCertificateGetAllDto>> GetTechCertDataAll(long PId)
        {
            return await productAppService.GetTechCertDataAll(PId);
        }
        [HttpGet]
        public async Task<List<ProductPriceDto>> GetPriceDataAll(long PSId)
        {
            return await productAppService.GetPriceDataAll(PSId);
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
        [HttpGet]
        public async Task<ResponseMessageDto> ProdDelete(long Id)
        {
            return await productAppService.ProdDelete(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> StockDelete(long Id)
        {
            return await productAppService.StockDelete(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> PriceDelete(long Id)
        {
            return await productAppService.PriceDelete(Id);
        }
    }
}
