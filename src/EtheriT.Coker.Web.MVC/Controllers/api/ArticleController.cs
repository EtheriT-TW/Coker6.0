using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Shared.Article;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ArticleController : Controller
    {
        private readonly IArticleAppService articleAppService;

        public ArticleController(IArticleAppService articleAppService)
        {
            this.articleAppService = articleAppService;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await articleAppService.GetAllList(loadOptions);
        }
    }
}
