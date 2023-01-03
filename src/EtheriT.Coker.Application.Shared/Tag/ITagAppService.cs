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
        public Task<ResponseMessageDto> TagAddUp(DevExpressDto dto);
        public Task<ResponseMessageDto> TagAssociateAddDelect(List<TagAssociateDto> dto);
        public Task<List<TagGetAllDataDto>> GetProductDataAll(long PId);
        public Task<ResponseMessageDto> TagDelete(long Id);
        public Task<ResponseMessageDto> TagAssociateDelete(long Id);

    }
}
