
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderRepaySetDto
    {
        public long ohid {  get; set; }
        public decimal Subtotal { get; set; }
        public List<OrderRepaySetDetailsDto>? Details { get; set; }
    }
}
