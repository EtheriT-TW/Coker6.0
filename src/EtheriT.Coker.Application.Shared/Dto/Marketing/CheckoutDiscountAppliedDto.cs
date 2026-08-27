using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CheckoutDiscountAppliedDto
    {
        public CheckoutDiscountSourceTypeEnum SourceType { get; set; }

        public long? CampaignId { get; set; }

        public long? RuleId { get; set; }

        public long? CouponId { get; set; }

        public string Name { get; set; } = "";

        public MarketingCampaignTypeEnum? CampaignType { get; set; }

        public MarketingRuleTypeEnum? RuleType { get; set; }

        public decimal BaseAmount { get; set; }

        public decimal ThresholdAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public int AppliedTimes { get; set; }

        public string DisplayText { get; set; } = "";
    }
}
