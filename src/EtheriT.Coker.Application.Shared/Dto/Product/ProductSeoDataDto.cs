using EtheriT.Coker.Application.Shared.Dto.enumType.Product;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    /// <summary>
    /// 商品頁 SEO 使用的公開資料。價格固定取非會員現金價，不受目前登入角色影響。
    /// </summary>
    public class ProductSeoDataDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ItemNo { get; set; }
        public decimal? PublicPrice { get; set; }
        public bool IsAvailable { get; set; }
        public List<ProductSeoVariantDto> Variants { get; set; } = new();
    }

    public class ProductSeoVariantDto
    {
        public long StockId { get; set; }
        public string? SubItemNo { get; set; }
        public decimal PublicPrice { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageUrl { get; set; }
        public List<ProductSeoVariantOptionDto> Options { get; set; } = new();
    }

    public class ProductSeoVariantOptionDto
    {
        public string TypeName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public SeoVariantPropertyEnum SeoVariantProperty { get; set; }
    }
}
