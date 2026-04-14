
using EtheriT.Coker.Application.Shared.Dto.Freight;

namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class ShoppingCartDisplayDto
    {
        public bool Available { get; set; } = true;
        public long PId { get; set; }
        public long SCId { get; set; }
        public long PSId { get; set; }
        public long? PPId { get; set; }
        public string Title { get; set; }
        public string OldTitle { get; set; }
        public string Describe { get; set; }
        public string S1Title { get; set; }
        public string S2Title { get; set; }
        public string ImagePath { get; set; }
        // 價格前綴字(目前僅購物車有用到)
        public string? PriceLabel { get; set; }
        public decimal Price { get; set; }
        public decimal DynamicPrice { get; set; }
        public decimal OldPrice { get; set; }
        public decimal Discont { get; set; }
        public int Bonus { get; set; }
        public int Quantity { get; set; }
        public int Step { get; set; }
        public int OldQuantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal SubtotalBonus { get; set; }
        public int Stock { get; set; }
        public int PackingPoint { get; set; } = 1;
        public string? LogisticsSubType { get; set; }
        public string? CVSStoreID { get; set; }
        public string? CVSStoreName { get; set; }
        public string? CVSAddress { get; set; }
        public string? CVSTelephone { get; set; }
        public string? CVSOutSide { get; set; }
        public FreightGetAllListDto? Freight { get; set; }
    }
}
