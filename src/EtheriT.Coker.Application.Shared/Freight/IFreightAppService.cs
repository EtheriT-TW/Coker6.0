
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Freight
{
    public interface IFreightAppService
    {
        public Task<ResponseMessageDto> AddUp(FreightDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetLogisticsBoxAllList(DataSourceLoadOptions loadOptions);
        public Task<FreightDto> GetOne(long Id);
        public Task<JsonResult> GetDisplay();
        public Task<ResponseMessageDto> Delete(long Id);

    }
}
