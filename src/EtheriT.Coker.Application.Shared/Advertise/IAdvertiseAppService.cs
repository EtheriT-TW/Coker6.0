using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
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
        public Task<ResponseMessageDto> GetConten(SearchIDDto dto);
        public Task<ResponseMessageDto> ImportConten(ArticleSaveContenDto dto);
        public Task<ResponseMessageDto> SaveConten(ArticleSaveContenDto dto);
    }
}
