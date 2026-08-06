namespace EtheriT.Coker.Application.Shared.Dto.ShoppingCart
{
    public class ShoppingCartAddOnSelectionUpdateDto
    {
        public long CampaignId { get; set; }
        public long RuleId { get; set; }
        public List<ShoppingCartRewardSelectionDto> Selections { get; set; } = new();
    }
}
