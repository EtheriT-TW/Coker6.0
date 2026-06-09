using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Dto.Marketing;
using EtheriT.Coker.Application.Shared.Marketing;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Core.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MarketingController : ControllerBase
    {
        private readonly IMarketingAppService _marketingAppService;

        public MarketingController(IMarketingAppService marketingAppService)
        {
            _marketingAppService = marketingAppService;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await _marketingAppService.GetAllList(loadOptions);
        }

        [HttpGet]
        public async Task<IActionResult> GetOne(long id)
        {
            var result = await _marketingAppService.GetOne(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddUp(MarketingCampaignEditDto input)
        {
            var result = await _marketingAppService.AddUp(input);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _marketingAppService.Delete(id);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetOptions()
        {
            var result = await _marketingAppService.GetOptions();
            return Ok(result);
        }
    }
}