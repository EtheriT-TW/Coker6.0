using EtheriT.Coker.Application.Shared.Dto.Tag;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Tag;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TagsController : Controller
    {
        private readonly ITagAppService tagAppService;
        public TagsController(ITagAppService tagAppService)
        {
            this.tagAppService = tagAppService;
        }
        [HttpGet]
        public async Task<List<TagGetAllDataDto>> GetArticleDataAll(long AId)
        {
            return await tagAppService.GetArticleDataAll(AId);
        }
    }
}
