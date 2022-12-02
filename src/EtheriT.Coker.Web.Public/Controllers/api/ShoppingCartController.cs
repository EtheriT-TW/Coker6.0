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
        public ShoppingCartController(
            IShoppingCartAppService shoppingCartAppService
            )
        {
            this.shoppingCartAppService = shoppingCartAppService;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(ShoppingCartAddUpDto dto)
        {
            return await shoppingCartAppService.AddUp(dto);
        }

        [HttpGet]
        public async Task<List<ShoppingCartGetDrop>> GetDrop(String id)
        {
            return await shoppingCartAppService.GetDrop(id);
        }

        [HttpGet]
        public async Task<ResponseMessageDto> DeleteDrop(long id)
        {
            return await shoppingCartAppService.DeleteDrop(id);
        }

    }
}
