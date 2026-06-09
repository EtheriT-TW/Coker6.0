namespace EtheriT.Coker.Application.Shared.Dto.enumType.Marketing
{
    /// <summary>
    /// 行銷活動大類。
    /// 用於 MarketingCampaign.CampaignType，表示此活動在後台歸屬於哪一種行銷活動類型。
    /// 此 enum 主要用於活動分類、後台畫面切換與未來擴充，不直接代表實際折扣計算方式。
    /// 實際計算方式應由 MarketingRuleTypeEnum、MarketingConditionTypeEnum、MarketingDiscountTypeEnum 等欄位決定。
    /// </summary>
    public enum MarketingCampaignTypeEnum
    {
        /// <summary>
        /// 滿額優惠。
        /// 表示活動以消費金額作為主要觸發條件。
        /// 例如：全站滿 1000 折 100、全站滿 2000 打 9 折。
        /// 第一階段主要使用此類型。
        /// </summary>
        滿額優惠 = 0,

        /// <summary>
        /// 滿件優惠。
        /// 表示活動以購買件數作為主要觸發條件。
        /// 例如：滿 3 件折 100、滿 5 件打 8 折。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        滿件優惠 = 10,

        /// <summary>
        /// 指定商品優惠。
        /// 表示活動只套用於指定商品、指定規格或指定分類。
        /// 例如：指定商品滿 1000 折 100、指定商品滿 3 件打 9 折。
        /// 第一階段暫不實作，未來會搭配 MarketingScopeTypeEnum 與 MarketingScopeItem 使用。
        /// </summary>
        指定商品優惠 = 20,

        /// <summary>
        /// 加價購活動。
        /// 表示達成條件後，允許使用者用指定價格購買加價購商品。
        /// 例如：購買指定商品後，可用 199 元加購指定商品。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        加價購 = 30,

        /// <summary>
        /// 贈品活動。
        /// 表示達成條件後，贈送指定商品或指定規格商品。
        /// 例如：滿 2000 送贈品、購買指定商品送贈品。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        贈品活動 = 40,

        /// <summary>
        /// 免運活動。
        /// 表示達成條件後，免除整筆訂單或指定範圍商品的運費。
        /// 例如：全站滿 1000 免運、指定商品免運。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        免運活動 = 50,

        /// <summary>
        /// 推薦商品。
        /// 表示活動主要用於前台展示推薦商品，不一定會影響訂單金額。
        /// 例如：購物車推薦商品、商品頁搭配推薦。
        /// 第一階段暫不實作，保留未來擴充。
        /// </summary>
        推薦商品 = 60,
    }
}