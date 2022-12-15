using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.HtmlContent;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.SideFloating
{
    public class SideFloating : ViewComponent
    {

        private readonly IHtmlContentAppService htmlContentAppService;
        private readonly IConfiguration Configuration;
        public SideFloating(
            IHtmlContentAppService htmlContentAppService,
            IConfiguration Configuration
            )
        {
            this.htmlContentAppService = htmlContentAppService;
            this.Configuration = Configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var siteId = Configuration.GetValue<long>("WebConfig:SiteId");
            var rightSideAd = JsonConvert.DeserializeObject<List<HtmlContentDisplayDto>>(JsonConvert.SerializeObject((await htmlContentAppService.GetDisplay(siteId, 12, 4)).Value));
            SideFloatingViewModel model = new SideFloatingViewModel
            {
                rightSideAd = rightSideAd
            };
            return View(model);
        }
    }
}