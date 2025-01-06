using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDisplayDto : ResponseMessageDto
    {
        public OrderHeaderDisplayDto OrderHeader { get; set; }
        public List<OrderDetailDisplayDto> OrderDetails { get; set; }
    }
}
