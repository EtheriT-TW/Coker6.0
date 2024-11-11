using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderAgainDto : ResponseMessageDto
    {
        public List<ShoppingCartGetDrop> OutOfStockDetails { get; set; }
    }
}
