using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Tag
{
    public interface ITagAppService
    {
        public Task<ResponseMessageDto> TagAddUp(DevExpressDto dto);
        public Task<ResponseMessageDto> TagGroupAddUp(DevExpressDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions, string? tids);
        public Task<JsonResult> GetAllGroupList(DataSourceLoadOptions loadOptions);
        public IQueryable<TagGroupLookupDto> TagGroupLookUp(long websiteId);
        public Task<ResponseMessageDto> TagAssociateAddDelect(List<TagAssociateDto> dto);
        public Task<List<TagGetSelectedDto>> GetTagAssociate(TagAssociateGetDto dto);
        public Task<List<TagGetAllDataDto>> GetProductDataAll(long PId);
        public Task<List<TagGetAllDataDto>> GetAdvertiseDataAll(long AdId);
        public Task<TagGetAllListDto?> GetTagByName(string name);
        public Task<ResponseMessageDto> TagDelete(long Id);
        public Task<ResponseMessageDto> TagGroupDelete(long Id);
        public Task<ResponseMessageDto> TagAssociateDelete(long AId);
    }
}
