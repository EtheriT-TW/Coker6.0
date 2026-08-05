using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class MarketingCampaignListDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public MarketingCampaignTypeEnum CampaignType { get; set; }

        public MarketingDisplayStatusEnum Status { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool NeverEnd { get; set; }

        public int Priority { get; set; }

        public bool CanStack { get; set; }

        public bool Repeatable { get; set; }

        public MarketingRuleTypeEnum RuleType { get; set; }

        public MarketingConditionTypeEnum ConditionType { get; set; }

        public decimal? MinAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? DiscountPercent { get; set; }

        public int ScopeItemCount { get; set; }

        public int RewardItemCount { get; set; }

        public decimal? MinOfferPrice { get; set; }

        public decimal? MaxOfferPrice { get; set; }
    }
}
