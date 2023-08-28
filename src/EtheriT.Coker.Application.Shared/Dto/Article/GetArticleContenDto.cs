using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.Dto.Article
{
    public class GetArticleContenDto : ResponseMessageDto
    {
        public string? Title { get; set; }
        public ArticleSaveContenDto? Conten { get; set; }
    }
}
