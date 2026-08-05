using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class MarketingCampaignEditDto
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public MarketingCampaignTypeEnum CampaignType { get; set; }

        public MarketingDisplayStatusEnum Status { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool NeverEnd { get; set; }

        public int Priority { get; set; }

        public bool CanStack { get; set; }

        public bool Repeatable { get; set; }

        public MarketingRuleTypeEnum RuleType { get; set; }

        public MarketingConditionTypeEnum ConditionType { get; set; } = MarketingConditionTypeEnum.OrderAmount;

        public MarketingScopeTypeEnum ScopeType { get; set; } = MarketingScopeTypeEnum.AllOrder;

        public decimal? MinAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? DiscountPercent { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public int SelectionQuantityPerQualification { get; set; } = 1;

        public int? MaxSelectionQuantityPerOrder { get; set; }

        public List<MarketingScopeItemEditDto> ScopeItems { get; set; } = new();

        public List<MarketingRewardItemEditDto> RewardItems { get; set; } = new();
    }
}
