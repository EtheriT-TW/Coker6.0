using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderGetDisplayDataOneDto : ResponseMessageDto
    {
        public OrderHeaderDisplayDto OrderHeader { get; set; }
        public List<OrderDetailsGetAllDto> OrderDetails { get; set; }
    }
}
