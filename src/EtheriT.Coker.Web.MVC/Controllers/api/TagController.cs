using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TagController : Controller
    {
        private readonly ITagAppService tagAppService;
        private readonly LoginUserData loginUserData;
        public TagController(
            ITagAppService tagAppService,
            LoginUserData loginUserData
        )
        {
            this.tagAppService = tagAppService;
            this.loginUserData = loginUserData;
        }
        [HttpPost]
        //[ValidateAntiForgeryToken] //驗證異常暫時關閉
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ResponseMessageDto> TagAddUp([FromForm] DevExpressDto dto)
        {
            return await tagAppService.TagAddUp(dto);
        }
        [HttpPost]
        //[ValidateAntiForgeryToken] //驗證異常暫時關閉
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ResponseMessageDto> TagGroupAddUp([FromForm] DevExpressDto dto)
        {
            return await tagAppService.TagGroupAddUp(dto);
        }
        [HttpGet]
        //[ValidateAntiForgeryToken] //驗證異常暫時關閉
        public async Task<IActionResult> TagGroupLookUp(DataSourceLoadOptions loadOptions) {
            long webid = await loginUserData.GetWebsiteId();
            var query = tagAppService.TagGroupLookUp(webid);
            var result = await DataSourceLoader.LoadAsync(query, loadOptions);
            return Json(result);
        }   
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions, string? tids)
        {
            return await tagAppService.GetAllList(loadOptions, tids);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllGroupList(DataSourceLoadOptions loadOptions)
        {
            return await tagAppService.GetAllGroupList(loadOptions);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> TagAssociateAddDelect(List<TagAssociateDto> dto)
        {
            return await tagAppService.TagAssociateAddDelect(dto);
        }
        [HttpGet]
        public async Task<List<TagGetAllDataDto>> GetProductDataAll(long PId)
        {
            return await tagAppService.GetProductDataAll(PId);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> TagDelete(long Id)
        {
            return await tagAppService.TagDelete(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> TagGroupDelete(long Id)
        {
            return await tagAppService.TagGroupDelete(Id);
        }
    }
}
