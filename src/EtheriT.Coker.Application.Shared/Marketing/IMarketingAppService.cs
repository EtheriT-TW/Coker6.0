using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Marketing;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Application.Shared.Marketing
{
    public interface IMarketingAppService
    {
        public Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions);

        public Task<ResponseMessageDto> GetOne(long id);

        public Task<ResponseMessageDto> AddUp(MarketingCampaignEditDto input);

        public Task<ResponseMessageDto> Delete(long id); 
        public Task<ResponseMessageDto> GetOptions();
        public Task<ResponseMessageDto> GetCartMarketingCampaigns();
    }
}