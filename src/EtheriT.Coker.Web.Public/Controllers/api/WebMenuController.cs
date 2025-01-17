using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.Public.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WebMenuController : Controller
    {
        private readonly IWebMenuApplication webMenuApplication;
        public WebMenuController(IWebMenuApplication webMenuApplication) { this.webMenuApplication = webMenuApplication; }
        [HttpGet]
        public async Task<SiteMapDto> GetAll()
        {
            return await webMenuApplication.GetSiteMap();
        }
    }
}
