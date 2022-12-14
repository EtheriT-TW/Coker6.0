using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;
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
        public async Task<ProdGetOneDto> GetDisplayOne(long id)
        {
            return await productAppService.GetDisplayOne(id);
        }
        [HttpGet]
        public async Task<List<long>> GetRandomId(int num)
        {
            return await productAppService.GetRandomId(num);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ClickLog(ProductLogDto dto)
        {
            return await productAppService.ClickLog(dto);
        }
    }
}
