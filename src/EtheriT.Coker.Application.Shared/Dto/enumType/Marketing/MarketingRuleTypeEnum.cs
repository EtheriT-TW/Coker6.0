namespace EtheriT.Coker.Application.Shared.Dto.enumType.Marketing
{
    /// <summary>
    /// 行銷活動規則類型。
    /// 用於 MarketingRule.RuleType，表示這一筆規則實際要執行哪一種優惠邏輯。
    /// CampaignType 是活動大類，RuleType 則是實際計算規則。
    /// 例如 CampaignType = 滿額優惠，而 RuleType 可以是 AmountDiscount 或 PercentDiscount。
    /// </summary>
    public enum MarketingRuleTypeEnum
    {
        /// <summary>
        /// 滿額折固定金額。
        /// 表示達到指定金額門檻後，折抵固定金額。
        /// 例如：滿 1000 折 100。
        /// 第一階段主要使用此規則。
        /// </summary>
        AmountDiscount = 1,

        /// <summary>
        /// 滿額打折。
        /// 表示達到指定金額門檻後，依百分比折扣計算。
        /// 例如：滿 2000 打 9 折。
        /// 第一階段主要使用此規則。
        /// </summary>
        PercentDiscount = 2,

        /// <summary>
        /// 免運規則。
        /// 表示達成條件後，免除運費。
        /// 例如：全站滿額免運、指定商品免運。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        FreeShipping = 10,

        /// <summary>
        /// 贈品規則。
        /// 表示達成條件後，贈送指定商品或規格。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        GiftProduct = 20,

        /// <summary>
        /// 加價購規則。
        /// 表示達成條件後，開放指定商品以加價購價格購買。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        AddOnPurchase = 30,

        /// <summary>
        /// 發送優惠券規則。
        /// 表示達成條件後，不在本次訂單直接折抵，而是發送優惠券供後續使用。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        CouponReward = 40,

        /// <summary>
        /// 發送紅利規則。
        /// 表示達成條件後，發送會員紅利點數。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        BonusReward = 50,

        /// <summary>
        /// 買 X 送 Y 規則。
        /// 表示購買指定數量或指定商品後，贈送另一商品或折抵另一商品。
        /// 例如：買三送一。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        BuyXGetY = 60,

        /// <summary>
        /// 任選組合規則。
        /// 表示在指定商品池中任選指定數量後，套用組合價、固定折抵或百分比折扣。
        /// 例如：任選 N 件折抵、紅白配。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        MixAndMatch = 70,

        /// <summary>
        /// 推薦商品規則。
        /// 表示此規則主要用於前台展示推薦商品，不一定會影響訂單金額。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        RecommendProduct = 80
    }
}