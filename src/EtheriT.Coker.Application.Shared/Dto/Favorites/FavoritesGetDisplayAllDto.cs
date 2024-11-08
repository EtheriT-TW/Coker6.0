
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Product;

namespace EtheriT.Coker.Application.Shared.Dto.Favorites
{
    public class FavoritesGetDisplayAllDto : ResponseMessageDto
    {
        public List<FavoritesGetDisplayOneDto> Data { get; set; }
        public int Page_Total { get; set; }
    }
}
