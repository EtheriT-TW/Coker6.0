namespace EtheriT.Coker.Application.Shared.Dto.enumType.Marketing
{
    /// <summary>
    /// 行銷活動範圍目標類型。
    /// 用於 MarketingScopeItem.TargetType，表示 TargetId 對應的是哪一種資料。
    /// 例如 TargetType = Product 時，TargetId 就是商品 Id。
    /// </summary>
    public enum MarketingScopeTargetTypeEnum
    {
        /// <summary>
        /// 商品。
        /// 表示 MarketingScopeItem.TargetId 對應商品主檔 Id。
        /// 用於指定商品優惠、指定商品免運、指定商品加價購等情境。
        /// </summary>
        Product = 1,

        /// <summary>
        /// 商品規格。
        /// 表示 MarketingScopeItem.TargetId 對應商品規格或庫存規格 Id。
        /// 用於只針對特定規格套用優惠的情境。
        /// </summary>
        ProductStock = 2,

        /// <summary>
        /// 商品分類。
        /// 表示 MarketingScopeItem.TargetId 對應分類 Id。
        /// 用於指定分類滿額、指定分類折扣等情境。
        /// </summary>
        Category = 3,

        /// <summary>
        /// 品牌。
        /// 表示 MarketingScopeItem.TargetId 對應品牌 Id。
        /// 目前若系統尚未建立品牌資料，可先保留不使用。
        /// </summary>
        Brand = 4,

        /// <summary>
        /// 商品標籤。
        /// 表示 MarketingScopeItem.TargetId 對應商品標籤 Id。
        /// 用於依標籤建立活動範圍的情境。
        /// </summary>
        ProductTag = 5
    }
}