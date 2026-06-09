namespace EtheriT.Coker.Application.Shared.Dto.enumType.Marketing
{
    /// <summary>
    /// 行銷活動適用範圍類型。
    /// 用於 MarketingRule.ScopeType，表示此規則要套用於整張訂單，或只套用於指定商品、規格、分類等範圍。
    /// 若 ScopeType 不是 AllOrder，通常需要搭配 MarketingScopeItem 記錄實際目標。
    /// </summary>
    public enum MarketingScopeTypeEnum
    {
        /// <summary>
        /// 全站 / 整張訂單。
        /// 表示此規則適用於整筆訂單，不限制商品、規格或分類。
        /// 第一階段全站滿額優惠使用此類型。
        /// 此類型通常不需要建立 MarketingScopeItem。
        /// </summary>
        AllOrder = 1,

        /// <summary>
        /// 指定商品。
        /// 表示此規則只適用於指定商品。
        /// 未來需搭配 MarketingScopeItem，並以 TargetType = Product 記錄商品 Id。
        /// </summary>
        SpecificProducts = 2,

        /// <summary>
        /// 指定商品規格。
        /// 表示此規則只適用於指定商品規格或庫存規格。
        /// 未來需搭配 MarketingScopeItem，並以 TargetType = ProductStock 記錄規格 Id。
        /// </summary>
        SpecificProductStocks = 3,

        /// <summary>
        /// 指定分類。
        /// 表示此規則只適用於指定商品分類底下的商品。
        /// 未來需搭配 MarketingScopeItem，並以 TargetType = Category 記錄分類 Id。
        /// </summary>
        SpecificCategories = 4,

        /// <summary>
        /// 指定品牌。
        /// 表示此規則只適用於指定品牌商品。
        /// 目前系統若尚未建立品牌資料，可先保留不使用。
        /// </summary>
        SpecificBrands = 5,

        /// <summary>
        /// 指定商品標籤。
        /// 表示此規則只適用於指定商品標籤底下的商品。
        /// 例如新品、熱銷、指定活動標籤。
        /// 目前若尚未建立商品標籤機制，可先保留不使用。
        /// </summary>
        SpecificProductTags = 6
    }
}