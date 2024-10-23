using DevExtreme.AspNet.Mvc;
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
        public TagController(
            ITagAppService tagAppService
            )
        {
            this.tagAppService = tagAppService;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ResponseMessageDto> TagAddUp([FromForm] DevExpressDto dto)
        {
            return await tagAppService.TagAddUp(dto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ResponseMessageDto> TagGroupAddUp([FromForm] DevExpressDto dto)
        {
            return await tagAppService.TagGroupAddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await tagAppService.GetAllList(loadOptions);
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
