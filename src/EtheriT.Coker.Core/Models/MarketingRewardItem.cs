using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// 行銷活動的可選優惠商品明細。
    /// 活動價為 0 時是贈品，大於 0 時是加價購商品。
    /// </summary>
    public class MarketingRewardItem : FullAuditedEntity
    {
        /// <summary>
        /// 所屬優惠結果。
        /// </summary>
        public long FK_MarketingRewardId { get; set; }

        public virtual MarketingReward MarketingReward { get; set; }

        /// <summary>
        /// 實際販售及扣庫存的商品規格（SKU）。
        /// </summary>
        public long FK_ProdStockId { get; set; }

        public virtual Prod_Stock ProdStock { get; set; }

        /// <summary>
        /// 活動成交價。0 表示贈品，大於 0 表示加價購。
        /// </summary>
        public decimal OfferPrice { get; set; }

        /// <summary>
        /// 此 SKU 在單筆訂單內最多可選取的數量。
        /// </summary>
        public int MaxQuantityPerOrder { get; set; } = 1;

        /// <summary>
        /// 是否啟用此優惠商品。
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 前台顯示順序，數字越小越前面。
        /// </summary>
        public int SortOrder { get; set; }
    }
}
