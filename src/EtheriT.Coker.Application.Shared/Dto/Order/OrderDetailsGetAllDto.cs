
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
	public class OrderDetailsGetAllDto
    {
        public long PId { get; set; }
        public long PSId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ImagePath { get; set; }
        public string? S1Title { get; set; }
        public string? S2Title { get; set; }
        public decimal Price { get; set; }
        public decimal? BonusPrice { get; set; }
        public decimal? SCPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }
}
