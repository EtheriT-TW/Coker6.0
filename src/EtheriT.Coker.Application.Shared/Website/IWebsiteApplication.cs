using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Webs.Dto;

namespace EtheriT.Coker.Application
{
    public interface IWebsiteApplication
    {
        public Task<List<WebsDto>> GetAll();
        public Task<int> GetLayoutType(long Id);
        public Task<string> GetOrgName(long Id);
        public Task<long> GetSiteId(long father_id, string key);
        public Task<ResponseMessageDto> Exchange(WebExchangeDto dto);
    }
}
