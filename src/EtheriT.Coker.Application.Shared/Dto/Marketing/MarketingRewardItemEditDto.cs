namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class MarketingRewardItemEditDto
    {
        public long Id { get; set; }

        public long ProductId { get; set; }

        public long ProductStockId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string StockName { get; set; } = string.Empty;

        public string Sku { get; set; } = string.Empty;

        public decimal OriginalPrice { get; set; }

        public int ProductStatus { get; set; }

        public string ProductStatusName { get; set; } = string.Empty;

        public bool Visible { get; set; }

        public bool Available { get; set; }

        public bool NoStockManagement { get; set; }

        public int? StockQuantity { get; set; }

        public int? AlertQuantity { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public decimal OfferPrice { get; set; }

        public int MaxQuantityPerOrder { get; set; } = 1;

        public bool Enabled { get; set; } = true;

        public int SortOrder { get; set; }
    }
}
