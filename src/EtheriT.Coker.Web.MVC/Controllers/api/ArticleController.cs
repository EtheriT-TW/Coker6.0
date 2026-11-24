using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Article;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;
using EtheriT.Coker.Web.MVC.Startup;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class ArticleController : Controller
    {
        private readonly IArticleAppService articleAppService;
        private readonly NavigationProvider navigation;
        private readonly IAntiforgery antiforgery;

        public ArticleController(IArticleAppService articleAppService, NavigationProvider navigation, IAntiforgery antiforgery)
        {
            this.articleAppService = articleAppService;
            this.navigation = navigation;
            this.antiforgery = antiforgery;
        }
        [HttpPost]
        public async Task<ResponseMessageDto> AddUp(ArticleDto dto)
        {
            return await articleAppService.AddUp(dto);
        }
        [HttpGet]
        public async Task<JsonResult> GetAllList(DataSourceLoadOptions loadOptions)
        {
            return await articleAppService.GetAllList(loadOptions);
        }
        [HttpGet]
        public async Task<ArticleGetDataDto> GetDataOne(long Id)
        {
            return await articleAppService.GetDataOne(Id);
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
            return await FilterCleanupOfferAsync(await articleAppService.ImportConten(dto));
        }
        [HttpPost]
        public async Task<ResponseMessageDto> SaveConten(ArticleSaveContenDto dto)
        {
            return await FilterCleanupOfferAsync(await articleAppService.SaveConten(dto));
        }
        private static bool CleanupFeatureEnabled(IEnumerable<JobMenu> jobs, string controller, string action)
        {
            foreach (var job in jobs)
            {
                if (!job.Enable) continue;
                if (job.Controller == controller && job.Action == action) return true;
                if (job.jobItemModels != null && CleanupFeatureEnabled(job.jobItemModels, controller, action)) return true;
            }
            return false;
        }

        private async Task<bool> AllowedCleanupAsync()
        {
            var scope = await navigation.BuildDiagnosticsScopeAsync();
            if (!CleanupFeatureEnabled(scope.Jobs, "FileManagement", "Index")) return false;
            var site = await navigation.BuildAuthorizedSiteAsync(false, "FileManagement", "Index");
            var files = navigation.FindJob(site.Jobs, "FileManagement", "Index");
            if (files == null || !files.Enable || !files.CanVisble || !files.CanRemove) return false;
            var articles = navigation.FindJob(site.Jobs, "ContentManagement", "Article");
            var directories = navigation.FindJob(site.Jobs, "ContentManagement", "Directory");
            return (CleanupFeatureEnabled(scope.Jobs, "ContentManagement", "Article")
                    && articles is { Enable: true, CanVisble: true, CanUpdate: true })
                || (CleanupFeatureEnabled(scope.Jobs, "ContentManagement", "Directory")
                    && directories is { Enable: true, CanVisble: true, CanUpdate: true });
        }

        private async Task<ResponseMessageDto> FilterCleanupOfferAsync(ResponseMessageDto response)
        {
            if (response.Success && response.Object is ArticleImageCleanupOfferDto)
            {
                try { if (!await AllowedCleanupAsync()) response.Object = null; }
                catch { response.Object = null; }
            }
            return response;
        }

        [HttpGet]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> GetImageCleanupAntiforgeryToken()
        {
            if (!await AllowedCleanupAsync()) return Forbid();
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken, headerName = tokens.HeaderName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecycleUnusedImages([FromBody] ArticleImageCleanupInputDto dto)
        {
            if (!await AllowedCleanupAsync()) return Forbid();
            return Ok(await articleAppService.RecycleUnusedImages(dto));
        }

        [HttpPost]
        public async Task<ResponseMessageDto> RebuildContentWithFiles(long AId)
        {
            return await articleAppService.RebuildContentWithFiles(AId);
        }
        // 2026/05/14 21:15 正式站Run過一次 先隱藏
        // 2026/05/14 21:54 需重新Run 先開啟
        [HttpGet]
        public async Task<ResponseMessageDto> RebuildAllContentWithFiles()
        {
            return await articleAppService.RebuildAllContentWithFiles();
        }
    }
}
