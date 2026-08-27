using EtheriT.Coker.Application.Shared.Dto.enumType.Marketing;

namespace EtheriT.Coker.Application.Shared.Dto.Marketing
{
    public class MarketingScopeItemEditDto
    {
        public long Id { get; set; }

        public MarketingScopeTargetTypeEnum TargetType { get; set; } = MarketingScopeTargetTypeEnum.Product;

        public long TargetId { get; set; }

        public string TargetName { get; set; } = string.Empty;

        public int RequiredQuantityPerQualification { get; set; } = 1;

        public int ProductStatus { get; set; }

        public string ProductStatusName { get; set; } = string.Empty;

        public bool Visible { get; set; }

        public bool Available { get; set; }

        public bool NoStockManagement { get; set; }

        public int? StockQuantity { get; set; }

        public int? AlertQuantity { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
    }
}
