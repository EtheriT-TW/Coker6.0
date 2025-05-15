using DevExpress.CodeParser;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TemplateController : Controller
    {
        private readonly ITemplatesApplicationService templatesApplicationService;
        public TemplateController(ITemplatesApplicationService templatesApplicationService)
        {
            this.templatesApplicationService = templatesApplicationService;
        }
        [HttpGet]
        public async Task<ResponseMessageDto> getDefaultFooter()
        {
            return await templatesApplicationService.GetDefaultFooterTemplatesAsync();
        }
        [HttpPost]
        public async Task<ResponseMessageDto> saveDefaultFooter(MenuSaveContenDto dto)
        {
            return await templatesApplicationService.saveDefaultFooter(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> importDefaultFooter(MenuSaveContenDto dto)
        {
            return await templatesApplicationService.importDefaultFooter(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> saveDefaultHeader(HeaderTemplateDto dto)
        {
            return await templatesApplicationService.saveDefaultHeader(dto);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> getDefaultHeader() {
            return await templatesApplicationService.getDefaultHeader();
        }
    }
}
