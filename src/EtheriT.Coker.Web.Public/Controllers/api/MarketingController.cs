using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Marketing;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MarketingController : Controller
    {
        private readonly IMarketingAppService marketingAppService;

        public MarketingController(IMarketingAppService marketingAppService)
        {
            this.marketingAppService = marketingAppService;
        }

        [HttpPost]
        public async Task<ResponseMessageDto> GetCartMarketingCampaigns()
        {
            return await marketingAppService.GetCartMarketingCampaigns();
        }
    }
}