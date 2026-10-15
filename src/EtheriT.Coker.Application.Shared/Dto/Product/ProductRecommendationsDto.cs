namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProductRecommendationsDto
    {
        public long ProductId { get; set; }
        public bool IsPersonalized { get; set; }
        public double SimilarityWeight { get; set; }
        public double InterestWeight { get; set; }
        public List<ProductRecommendationItemDto> Items { get; set; } = new();
    }

    public class ProductRecommendationItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string Link { get; set; } = "";
        public int CommonTagCount { get; set; }
        public double SimilarityScore { get; set; }
        public double InterestScore { get; set; }
        public double Score { get; set; }
    }
}
