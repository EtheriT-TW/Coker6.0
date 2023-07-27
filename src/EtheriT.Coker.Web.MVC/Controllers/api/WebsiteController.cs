using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WebsiteController : Controller
    {
        private readonly IWebsiteApplication websiteApplication;
        public WebsiteController(IWebsiteApplication websiteApplication)
        {
            this.websiteApplication = websiteApplication;
        }
        [HttpPost]
        [Authorize]
        public async Task<ResponseMessageDto> Exchange(WebExchangeDto dto)
        {
            return await websiteApplication.Exchange(dto);
        }

        [HttpGet]
        [Authorize]
        public async Task<ResponseMessageDto> GetPrivacyAndTerms()
        {
            return await websiteApplication.GetPrivacyAndTerms();
        }
    }
}
