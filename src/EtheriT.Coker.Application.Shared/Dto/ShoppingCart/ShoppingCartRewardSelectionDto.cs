namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class ShoppingCartRewardSelectionDto
    {
        public long CampaignId { get; set; }
        public long RuleId { get; set; }
        public long RewardItemId { get; set; }
        public int Quantity { get; set; }
    }
}
