
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Favorites;

namespace EtheriT.Coker.Application.Shared.Dto.Product
{
    public class ProdGetHistoryDisplayAllDto : ResponseMessageDto
    {
        public List<DirectoryReleInfoDto> Data { get; set; }
        public int Page_Total { get; set; }
    }
}
