using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Tag
{
    public interface ITagAppService
    {
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> AddUp(DevExpressDto dto);
        public Task<List<TagGetAllDataDto>> GetProductDataAll(long PId);
        public Task<ResponseMessageDto> Delete(long Id);

    }
}
