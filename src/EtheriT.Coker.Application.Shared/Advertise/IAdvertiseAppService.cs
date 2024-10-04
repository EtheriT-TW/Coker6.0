using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Advertise
{
    public interface IAdvertiseAppService
    {
        public Task<ResponseMessageDto> AddUp(AdvertiseDto dto);
        public Task<JsonResult> GetList(DataSourceLoadOptions loadOptions, int Type);
        public Task<AdvertiseGetDataDto> GetDataOne(long Id);
        public Task<ResponseMessageDto> Delete(long Id);
        public  Task<ResponseMessageDto> ActivityLog(AdvertiseLogDto dto);
        public  Task<JsonResult> GetDisplay(long webid, int type, int number);
    }
}
