using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ShoppingCartController : Controller
    {

        private readonly IShoppingCartAppService shoppingCartAppService;
        private readonly IConfiguration Configuration;
        public ShoppingCartController(
            IShoppingCartAppService shoppingCartAppService,
            IConfiguration Configuration
            )
        {
            this.shoppingCartAppService = shoppingCartAppService;
            this.Configuration = Configuration;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto)
        {
            return await shoppingCartAppService.AddUp(dto);
        }

        [HttpPost]
        public async Task<ResponseMessageDto> QuantityUpdate(ShoppingQuantityUpdateDto dto)
        {
            return await shoppingCartAppService.QuantityUpdate(dto);
        }

        [HttpGet]
        public async Task<List<ShoppingCartGetAllDto>> GetAll(String Tid)
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            return await shoppingCartAppService.GetAll(Tid, siteId);
        }

        [HttpGet]
        public async Task<ShoppingCartGetDrop> GetDropOne(long id)
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            return await shoppingCartAppService.GetDropOne(id, siteId);
        }

        [HttpGet]
        public async Task<ResponseMessageDto> DeleteDrop(long id)
        {
            return await shoppingCartAppService.DeleteDrop(id);
        }

    }
}
