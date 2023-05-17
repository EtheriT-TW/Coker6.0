using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
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
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp_Simple(ArticleDto dto)
        {
            return await articleAppService.AddUp_Simple(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await articleAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<ArticleDataGetDto> GetSimple(long Id)
        {
            return await articleAppService.GetSimple(Id);
        }
        [HttpGet]
        public async Task<ResponseMessageDto> Delete(long Id)
        {
            return await articleAppService.Delete(Id);
        }
        [HttpPost]
        public async Task<GetArticleContenDto> GetConten(SearchIDDto dto)
        {
            return await articleAppService.GetConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> ImportConten(ArticleSaveContenDto dto)
        {
            return await articleAppService.ImportConten(dto);
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveConten(ArticleSaveContenDto dto)
        {
            return await articleAppService.SaveConten(dto);
        }
    }
}
