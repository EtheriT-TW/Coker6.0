
namespace EtheriT.Coker.Application.Shared.Dto.Order
{
    public class OrderDetailDisplayDto
    {
        public long PId { get; set; }
        public long ProdId { get; set; }
        public long SCId { get; set; }
        public long ProdPriceId { get; set; }
        public long ProdStockId { get; set; }
        public string Title { get; set; }
        public string OldTitle { get; set; }
        public string Describe { get; set; }
        public string S1Title { get; set; }
        public string S2Title { get; set; }
        public string ImagePath { get; set; }
        // 價格前綴字(目前僅購物車有用到)
        public string? PriceLabel { get; set; }
        public decimal Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal DynamicPrice { get; set; }
        public decimal Discont { get; set; }
        public int Bonus { get; set; }
        public int Quantity { get; set; }
        public int OldQuantity { get; set; }
        public int Step { get; set; }
        public decimal Subtotal { get; set; }
        public int SubtotalBonus { get; set; }
        public int Stock { get; set; }
        public bool IsAdditional { get; set; }
        public long? FK_MarketingRewardItemId { get; set; }
        public long? AdditionalParentShoppingCartId { get; set; }
        /// <summary>
        /// 加價購／贈品所依附的主商品；訂單滿額活動則顯示「本筆訂單」。
        /// </summary>
        public string? AdditionalParentLabel { get; set; }
    }
}
