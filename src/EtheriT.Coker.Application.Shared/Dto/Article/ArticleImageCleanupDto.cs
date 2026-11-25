namespace EtheriT.Coker.Application.Shared.Dto.Article;

public sealed class ArticleImageCleanupOfferDto
{
    public int Count { get; set; }
    public string? Token { get; set; }
    public bool CheckFailed { get; set; }
}

public sealed class ArticleImageCleanupInputDto
{
    public long Id { get; set; }
    public string Token { get; set; } = string.Empty;
}
