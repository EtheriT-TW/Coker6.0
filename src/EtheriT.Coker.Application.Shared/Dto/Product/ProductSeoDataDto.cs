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
    }
}
