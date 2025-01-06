using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Webs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class WebsiteController : Controller
    {
        private readonly IWebsiteApplication websiteApplication;
        public WebsiteController(IWebsiteApplication websiteApplication)
        {
            this.websiteApplication = websiteApplication;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> Exchange(WebExchangeDto dto)
        {
            return await websiteApplication.Exchange(dto);
        }

        [HttpGet]
        public async Task<ResponseMessageDto> GetPrivacyAndTerms()
        {
            return await websiteApplication.GetPrivacyAndTerms();
        }
        [HttpPost]
        public async Task<ResponseMessageDto> Save(WebsiteEditDto dto)
        {
            return await websiteApplication.Save(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> LoadFrameCss()
        {
            return await websiteApplication.LoadFrameCss();
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SettingCss(FrameCssDto dto)
        {
            return await websiteApplication.SettingCss(dto);
        }
    }
}
