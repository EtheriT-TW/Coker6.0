using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Favorites;
using EtheriT.Coker.Application.Shared.Dto.Product;

namespace EtheriT.Coker.Application.Shared.Favorites
{
    public interface IFavoritesAppService
    {
        public Task<ResponseMessageDto> Add(long Pid);
        public Task<FavoritesGetDisplayAllDto> GetDisplay(int page);
        public Task<ResponseMessageDto> Delete(long Fid);
        public Task<ResponseMessageDto> CheckIsFavorites(long Pid);
    }
}
