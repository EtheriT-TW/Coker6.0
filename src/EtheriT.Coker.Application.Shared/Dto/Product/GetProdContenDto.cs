
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class GetProdContenDto : ResponseMessageDto
    {
        public ProdSaveContenDto? Conten { get; set; }
    }
}
