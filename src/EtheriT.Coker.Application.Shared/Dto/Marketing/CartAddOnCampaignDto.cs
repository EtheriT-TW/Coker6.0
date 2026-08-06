using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class CartAddOnCampaignDto
    {
        public long CampaignId { get; set; }
        public long RuleId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public MarketingConditionTypeEnum ConditionType { get; set; }
        public decimal MinAmount { get; set; }
        public int RequiredQuantity { get; set; } = 1;
        public bool Repeatable { get; set; }
        public int SelectionQuantityPerQualification { get; set; } = 1;
        public List<long> ScopeProductIds { get; set; } = new();
        public List<ProductAddOnRewardItemDto> RewardItems { get; set; } = new();
    }
}
