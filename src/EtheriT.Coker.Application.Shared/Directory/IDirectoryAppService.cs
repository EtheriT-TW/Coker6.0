using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
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
        public Task<ResponseMessageDto> Delete(long Id);
    }
}
