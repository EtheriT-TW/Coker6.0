using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;

namespace EtheriT.Coker.Application.Shared.Advertise
{
    public interface IAdvertiseAppService
    {
        public Task<ResponseMessageDto> AddUp(AdvertiseDto dto);
        public Task<AdvertiseGetDataDto> GetDataOne(long Id);
        public Task<ResponseMessageDto> Delete(long Id);
        public  Task<ResponseMessageDto> ActivityLog(AdvertiseLogDto dto);
    }
}
