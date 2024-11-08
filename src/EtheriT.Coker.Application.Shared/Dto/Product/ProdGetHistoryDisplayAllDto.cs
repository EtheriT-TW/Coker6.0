
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Favorites;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProdGetHistoryDisplayAllDto : ResponseMessageDto
    {
        public List<ProdGetHistoryDisplayOneDto> Data { get; set; }
        public int Page_Total { get; set; }
    }
}
