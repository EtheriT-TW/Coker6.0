using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application
{
    public interface IWebMenuApplication
    {
        public Task<SiteMapDto> GetAll();
        public Task<SiteMapDto> GetDisplayAll(long WebsiteID);
        public Task<SiteMapDto> GetSiteMap();
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);
        public Task<MenuGetAllListDto> GetSelectData(long Mid);
        public Task<MenuItemDto> GetDisplayOne(DataIdWebsiteIdDto dto);
        public Task<ResponseMessageDto> CreateOrEdit(MenuItemDto dto);
        public Task<GetMenuContenDto> GetConten(SearchIDDto dto);
        public Task<GetFrontContenOutputDto> GetParentConten(GetFrontContenInputDto dto);
        public Task<GetFrontContenOutputDto> GetFrontConten(GetFrontContenInputDto dto);
        public Task<List<GetMenuBreadDto>> GetMenuBread(long Id);
        public Task<ResponseMessageDto> saveConten(MenuSaveContenDto dto);
        public Task<ResponseMessageDto> importConten(MenuSaveContenDto dto);
        public Task<ResponseMessageDto> Delete(DataDelectDto dto);
        public Task<ResponseMessageDto> updateSerNo(UpdateSerNoListDto dto);
        public PageTypeDto GetPageTypeList();
        public Task CheckDisplayAll(long WebsiteID);
        public Task insertMenus(List<SelectDto> menus);
        public Task<bool> checkHasShoppingCar(long WebsiteID);
		public Task<bool> checkHasMember(long WebsiteID);
        public Task<long> GetRootId(string name);
        public Task<List<JumpRuleDto>> GetJumpRulesAsync();
    }
}
