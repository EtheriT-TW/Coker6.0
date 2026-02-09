using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using EtheriT.Coker.Application.Webs.Dto;

namespace EtheriT.Coker.Application
{
    public interface IWebsiteApplication
    {
        public Task<DefaultDataDto> GetDefaultData(long siteId, string? website);
        public Task<List<WebsDto>> GetAll();
        public Task<WebsPageDto> GetPageAll(int? page);
        public Task<List<WebsiteDataDto>> GetAllData(long SiteId);
        public Task<ResponseMessageDto> GetPrivacyAndTerms();
        public Task<int> GetLayoutType(long Id);
        public Task<string> GetOrgName(long Id);
        public Task<long> GetSiteId(long father_id, string key);
        public Task<ResponseMessageDto> Exchange(WebExchangeDto dto);
        public Task<GetFrontContenOutputDto> GetPrivacyConten(GetFrontContenInputDto dto);
        public Task<WebsiteEditOutputDto> GetWebsiteData();
        public Task<ResponseMessageDto> Save(WebsiteEditDto dto);
        public Task<ResponseMessageDto> LoadFrameCss();
        public Task<ResponseMessageDto> SettingCss(FrameCssDto dto);
        public Task<string> GetDomain(long siteId=0);
        public Task<bool> hasManySystem();
    }
}
