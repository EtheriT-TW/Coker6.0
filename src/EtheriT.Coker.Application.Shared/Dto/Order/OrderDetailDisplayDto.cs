
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDetailDisplayDto
    {
        public long PId { get; set; }
        public long ProdId { get; set; }
        public long SCId { get; set; }
        public long PSId { get; set; }
        public long ProdStockId { get; set; }
        public string Title { get; set; }
        public string OldTitle { get; set; }
        public string Describe { get; set; }
        public string S1Title { get; set; }
        public string S2Title { get; set; }
        public string ImagePath { get; set; }
        public int Price { get; set; }
        public int OldPrice { get; set; }
        public int DynamicPrice { get; set; }
        public int Discont { get; set; }
        public int Bonus { get; set; }
        public int Quantity { get; set; }
        public int OldQuantity { get; set; }
        public int Step { get; set; }
        public int Subtotal { get; set; }
        public int Stock { get; set; }
    }
}
