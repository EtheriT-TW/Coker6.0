using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Shared.Favorites
{
    public interface IFavoritesAppService
    {
        public Task<ResponseMessageDto> Add(long Pid);
        public Task<ResponseMessageDto> Delete(long Fid);
    }
}
