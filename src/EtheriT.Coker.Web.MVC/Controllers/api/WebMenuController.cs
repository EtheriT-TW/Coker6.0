using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Authorization;
using EtheriT.Coker.Application.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WebMenuController : Controller
    {
        private readonly ILogger<WebMenuController> _logger;
        private readonly IWebMenuApplication webMenuApplication;
        public WebMenuController(ILogger<WebMenuController> logger, IWebMenuApplication webMenuApplication) {
            this._logger = logger;
            this.webMenuApplication = webMenuApplication;
        }
        [HttpGet]
        [Authorize]
        public async Task<SiteMapDto> GetAll()
        {
            return await webMenuApplication.GetAll();
        }
        [HttpPost]
        [Authorize]
        public async Task<ResponseMessageDto> CreateOrEdit(MenuItemDto dto)
        {
            return await webMenuApplication.CreateOrEdit(dto);
        }
        [HttpPost]
        [Authorize]
        public async Task<ResponseMessageDto> saveConten(MenuSaveContenDto dto)
        {
            return await webMenuApplication.saveConten(dto);
        }
        [HttpPost]
        [Authorize]
        public async Task<ResponseMessageDto> importConten(MenuContenDto dto)
        {
            return await webMenuApplication.importConten(dto);
        }
    }
}
