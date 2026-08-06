namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class ProductAddOnCampaignDto
    {
        public long CampaignId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int RequiredQuantity { get; set; } = 1;
        public int SelectionQuantityPerQualification { get; set; } = 1;
        public bool Repeatable { get; set; }
        public List<ProductAddOnRewardItemDto> RewardItems { get; set; } = new();
    }

    public class ProductAddOnRewardItemDto
    {
        public long RewardItemId { get; set; }
        public long ProductId { get; set; }
        public long ProductStockId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "/images/noImg.jpg";
        public decimal OriginalPrice { get; set; }
        public decimal OfferPrice { get; set; }
        public int MaxQuantityPerOrder { get; set; } = 1;
        public int? StockQuantity { get; set; }
        public bool NoStockManagement { get; set; }
    }
}
