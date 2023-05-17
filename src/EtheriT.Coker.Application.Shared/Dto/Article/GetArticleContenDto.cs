using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.Dto.Article
{
    public class GetArticleContenDto : ResponseMessageDto
    {
        public ArticleSaveContenDto? Conten { get; set; }
    }
}
