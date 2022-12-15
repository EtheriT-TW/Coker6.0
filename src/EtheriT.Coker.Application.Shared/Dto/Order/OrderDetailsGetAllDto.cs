
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
	public class OrderDetailsGetAllDto
    {
        public long PId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Subtotal { get; set; }
    }
}
