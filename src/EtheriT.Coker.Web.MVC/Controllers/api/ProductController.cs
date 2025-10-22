using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Article;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
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
        public async Task<ResponseMessageDto> ProductAddUp(ProdAddUpDto dto)
        {
            return await productAppService.ProductAddUp(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ProdPriceAddUp(List<ProductPriceDto> dto)
        {
            return await productAppService.PriceAddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions, [FromQuery] string? pids = null)
        {
            return await productAppService.GetAllList(loadOptions, pids);
        }
        [HttpGet]
        public async Task<JsonResult> SaleQuantityStaging(DataSourceLoadOptions loadOptions)
        {
            return await productAppService.SaleQuantityStaging(loadOptions);
        }
        [HttpGet]
        public async Task<ProdGetDataDto> GetProdDataOne(long Id)
        {
            return await productAppService.GetProdDataOne(Id);
        }
        [HttpGet]
        public async Task<List<ProductStockDto>> GetStockDataAll(long PId)
        {
            return await productAppService.GetStockDataAll(PId);
        }
        [HttpGet]
        public async Task<List<ProductPriceDto>> GetPriceDataAll(long PSId)
        {
            return await productAppService.GetPriceDataAll(PSId);
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
        [HttpPost]
        public async Task<ResponseMessageDto> ProdReplace(IList<IFormFile> files)
        {
            return await productAppService.ProdReplace(files);
        }
        [HttpPost]
        public async Task<GetProdContenDto> GetConten(SearchIDDto dto)
        {
            return await productAppService.GetConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ImportConten(ProdSaveContenDto dto)
        {
            return await productAppService.ImportConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveConten(ProdSaveContenDto dto)
        {
            return await productAppService.SaveConten(dto);
        }
    }
}
