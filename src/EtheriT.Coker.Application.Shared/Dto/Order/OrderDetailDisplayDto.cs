
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDetailDisplayDto
    {
        public long ProdId { get; set; }
        public long SCId { get; set; }
        public string Title { get; set; }
        public string Describe { get; set; }
        public string S1Title { get; set; }
        public string S2Title { get; set; }
        public string ImagePath { get; set; }
        public string Price { get; set; }
        public string Quantity { get; set; }
        public string Subtotal { get; set; }
    }
}
