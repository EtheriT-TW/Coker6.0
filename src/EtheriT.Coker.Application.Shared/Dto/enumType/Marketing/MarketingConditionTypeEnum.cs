namespace EtheriT.Coker.Application.Shared.Dto.enumType.Marketing
{
    /// <summary>
    /// 行銷活動觸發條件類型。
    /// 用於 MarketingCondition.ConditionType，表示此規則要用哪一種條件判斷是否符合活動資格。
    /// 第一階段主要使用 OrderAmount。
    /// </summary>
    public enum MarketingConditionTypeEnum
    {
        /// <summary>
        /// 訂單滿額。
        /// 表示以整張訂單的可計算金額判斷是否達到門檻。
        /// 例如：全站滿 1000 折 100。
        /// 門檻金額應存放於 MarketingCondition.MinAmount。
        /// 第一階段主要使用此條件。
        /// </summary>
        OrderAmount = 1,

        /// <summary>
        /// 指定範圍滿額。
        /// 表示只計算符合 Scope 的商品金額是否達到門檻。
        /// 例如：指定商品合計滿 1000 折 100。
        /// 未來需搭配 MarketingScopeItem 與 OnlyScopeItems 使用。
        /// </summary>
        ScopeAmount = 2,

        /// <summary>
        /// 訂單滿件。
        /// 表示以整張訂單的商品件數判斷是否達到門檻。
        /// 例如：全站滿 3 件折 100。
        /// 門檻件數應存放於 MarketingCondition.MinQuantity。
        /// </summary>
        OrderQuantity = 3,

        /// <summary>
        /// 指定範圍滿件。
        /// 表示只計算符合 Scope 的商品件數是否達到門檻。
        /// 例如：指定商品任選 3 件打 9 折。
        /// 未來需搭配 MarketingScopeItem 與 OnlyScopeItems 使用。
        /// </summary>
        ScopeQuantity = 4,

        /// <summary>
        /// 購買指定商品。
        /// 表示購物車內必須包含指定商品或指定規格才符合活動資格。
        /// 常用於加價購、買 A 送 B、購買指定商品送贈品等活動。
        /// 未來需搭配 MarketingScopeItem 使用。
        /// </summary>
        BuySpecificProduct = 5
    }
}