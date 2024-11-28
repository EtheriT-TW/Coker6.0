
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDetailDisplayDto
    {
        public long ProdId { get; set; }
        public long ProdStockId { get; set; }
        public long SCId { get; set; }
        public string Title { get; set; }
        public string Describe { get; set; }
        public string S1Title { get; set; }
        public string S2Title { get; set; }
        public string ImagePath { get; set; }
        public string Price { get; set; }
        public string? DynamicPrice { get; set; }
        public string Discont { get; set; }
        public string Bonus { get; set; }
        public string? OldPrice { get; set; }
        public string Quantity { get; set; }
        public string? OldQuantity { get; set; }
        public string Subtotal { get; set; }
    }
}
