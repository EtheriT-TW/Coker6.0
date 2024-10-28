using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Product;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductController : Controller
    {
        private readonly IProductAppService productAppService;
        public ProductController(
            IProductAppService productAppService
            )
        {
            this.productAppService = productAppService;
        }
        [HttpGet]
        public async Task<ProdGetMainDisplayDto> GetMainDisplayOne(long Id)
        {
            return await productAppService.GetMainDisplayOne(Id);
        }
        [HttpGet]
        public async Task<ProdGetDataDto> GetProdDataOne(long Id)
        {
            return await productAppService.GetProdDataOne(Id);
        }
        [HttpGet]
        public async Task<ProdGetOneDto> GetDisplayOne(long id)
        {
            return await productAppService.GetDisplayOne(id);
        }
        [HttpGet]
        public async Task<List<ProductStockDto>> GetDisplayStock(long id)
        {
            return await productAppService.GetDisplayStock(id);
        }
        [HttpGet]
        public async Task<ProdGetDisplayDto> GetDisplaySimple(long id)
        {
            return await productAppService.GetDisplaySimple(id);
        }
        [HttpGet]
        public async Task<List<ProdGetDisplayDto>> GetHistoryDisplay()
        {
            return await productAppService.GetHistoryDisplay();
        }
        [HttpGet]
        public async Task<ResponseMessageDto> ClickLog(long FK_Pid)
        {
            return await productAppService.ClickLog(FK_Pid);
        }
    }
}
