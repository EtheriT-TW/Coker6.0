using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Directory
{
    public interface IDirectoryAppService
    {
        public Task<ResponseMessageDto> AddUp(DirectoryAddUpDto dto);
        public Task<DirectoryGetDataDto> GetDataOne(long Id);
        public Task<DirectoryReleInfoGetDto> GetReleInfo(DirectoryReleInfoInputDto dto);
        public Task<MenuItemDto> GetReleMenu(DataIdWebsiteIdDto dto);
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetAdvertiseList(DataSourceLoadOptions loadOptions);
        public Task<ResponseMessageDto> Delete(long Id);
        public Task<JsonResult> GetDirectoryArticlesList(long id, DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetDirectoryProductsList(long id, DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetDirectoryMenusList(long id, DataSourceLoadOptions loadOptions);
        public Task<JsonResult> GetDirectoryAdvertiseList(long id, DataSourceLoadOptions loadOptions);
        public Task<List<AdvertiseDisplayDto>> GetReleAd(DataIdWebsiteIdDto dto);
        public Task<List<KeyValueDto>> SwitchPage(DirectorySwitchPageDto dto);
        public Task<ResponseMessageDto> GetDirectoryFacetConfig(long Id);
        public Task<ResponseMessageDto> SaveDirectoryFacetConfig(DirectoryFacetConfigDto dto);
        public Task<ResponseMessageDto> GetFacetAsync(long directoryId);
    }
}
