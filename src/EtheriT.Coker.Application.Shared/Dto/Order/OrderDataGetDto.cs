
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.ShoppingCart;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDataGetDto
    {
        public OrderHeaderGetOneDto OrderHeader {  get; set; }
        public List<ShoppingCartGetDrop> OrderDetails {  get; set; }
    }
}
