using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;
using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// 行銷活動優惠結果。
    /// 記錄符合 MarketingRule 條件後，實際要套用或發放的優惠內容。
    /// 優惠類型由 MarketingRule.RuleType 決定；本表只保存該優惠類型需要的參數。
    /// </summary>
    public class MarketingReward : FullAuditedEntity
    {
        public long FK_MarketingRuleId { get; set; }

        public virtual MarketingRule MarketingRule { get; set; }

        /// <summary>
        /// 優惠套用方式。
        /// 第一階段只使用 ApplyImmediately，表示本次訂單立即折抵。
        /// 未來若做優惠券、紅利，可擴充為付款完成後或訂單完成後發放。
        /// </summary>
        public MarketingRewardDeliveryTypeEnum DeliveryType { get; set; }

        /// <summary>
        /// 固定折抵金額。
        /// 當 MarketingRule.RuleType = AmountDiscount 時使用。
        /// 例如滿 1000 折 100，這裡存 100。
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        /// <summary>
        /// 折扣百分比。
        /// 當 MarketingRule.RuleType = PercentDiscount 時使用。
        /// 例如 90 = 九折、85 = 八五折。
        /// </summary>
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// 最高折抵金額。
        /// 百分比折扣未來可能需要使用。
        /// 例如滿 3000 打 9 折，但最高折 500。
        /// </summary>
        public decimal? MaxDiscountAmount { get; set; }

        /// <summary>
        /// 未來轉優惠券時使用。
        /// 當 RuleType = CouponReward 時，可指向優惠券模板。
        /// </summary>
        public long? FK_CouponTemplateId { get; set; }

        /// <summary>
        /// 未來送紅利時使用。
        /// 當 RuleType = BonusReward 時，記錄要發放的紅利點數。
        /// </summary>
        public int? BonusAmount { get; set; }

        /// <summary>
        /// 未來贈品活動使用。
        /// 當 RuleType = GiftProduct 時，記錄贈送的商品 Id。
        /// </summary>
        public long? FK_GiftProductId { get; set; }

        /// <summary>
        /// 未來贈品指定規格使用。
        /// 當贈品需要指定商品規格時，記錄贈品規格 Id。
        /// </summary>
        public long? FK_GiftProductStockId { get; set; }
    }
}