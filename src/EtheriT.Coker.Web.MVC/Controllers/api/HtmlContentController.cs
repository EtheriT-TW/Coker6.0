using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.EnterAd;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.HtmlContent;
using EtheriT.Coker.Application.Shared.Marquee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class HtmlContentController : Controller
    {

        private readonly IHtmlContentAppService htmlContentAppService;
        public HtmlContentController(
            IHtmlContentAppService htmlContentAppService
            )
        {
            this.htmlContentAppService = htmlContentAppService;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(HtmlContentDto dto)
        {
            return await htmlContentAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetEnterAdAllList(DataSourceLoadOptions loadOptions)
        {
            return await htmlContentAppService.GetAllList(8, loadOptions);
        }
        [HttpGet]
        public async Task<JsonResult> GetRightSideAdAllList(DataSourceLoadOptions loadOptions)
        {
            return await htmlContentAppService.GetAllList(12, loadOptions);
        }
        [HttpGet]
        public async Task<HtmlContentDto> GetOne(int id)
        {
            return await htmlContentAppService.GetOne(id);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> Delete(DataDelectDto dto)
        {
            return await htmlContentAppService.Delete(dto);
        }
        [HttpGet]
        public async Task<HtmlContentTypeDto> GetTypeList() {
            return await htmlContentAppService.GetTypeList();
        }
    }
}
