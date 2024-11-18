
namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class ShoppingCartDisplayDto
    {
        public long PId { get; set; }
        public long SCId { get; set; }
        public long FK_PSId { get; set; }
        public string Title { get; set; }
        public string Describe { get; set; }
        public string S1Title { get; set; }
        public string S2Title { get; set; }
        public string ImagePath { get; set; }
        public string Price { get; set; }
        public string Discont { get; set; }
        public string Bonus { get; set; }
        public string Quantity { get; set; }
        public string Subtotal { get; set; }
    }
}
