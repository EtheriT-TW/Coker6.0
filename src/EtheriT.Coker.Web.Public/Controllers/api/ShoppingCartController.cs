using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
using EtheriT.Coker.Application.Shared.ShoppingCart;
using EtheriT.Coker.Application.Shared.ThirdParty;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ShoppingCartController : Controller
    {

        private readonly IShoppingCartAppService shoppingCartAppService;
        private readonly IThirdPartyAppService thirdPartyAppService;
        private readonly IConfiguration Configuration;
        public ShoppingCartController(
            IShoppingCartAppService shoppingCartAppService,
            IThirdPartyAppService thirdPartyAppService,
            IConfiguration Configuration
            )
        {
            this.shoppingCartAppService = shoppingCartAppService;
            this.thirdPartyAppService = thirdPartyAppService;
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
            return await shoppingCartAppService.QuantityUpdate(new List<ShoppingQuantityUpdateDto> { dto });
        }
        [HttpPost]
        public async Task<ResponseMessageDto> MultiQuantityUpdate(List<ShoppingQuantityUpdateDto> dto)
        {
            var output = await shoppingCartAppService.QuantityUpdate(dto);
            return output;
        }

        [HttpGet]
        public async Task<List<ShoppingCartDisplayDto>> GetAll()
        {
            return await shoppingCartAppService.GetAll();
        }

        [HttpGet]
        public async Task<ShoppingCartDisplayDto> GetDropOne(long id, bool isorder)
        {
            return await shoppingCartAppService.GetDropOne(id, false);
        }

        [HttpGet]
        public async Task<ResponseMessageDto> DeleteDrop(long id)
        {
            return await shoppingCartAppService.DeleteDrop(id);
        }
        [HttpGet]
        public async Task<List<ThirdPartyKeypairItemOutputDto>> GetPaymentInfo(long paytypeid)
        {
            return await thirdPartyAppService.GetPaymentResult(paytypeid);
        }
    }
}
