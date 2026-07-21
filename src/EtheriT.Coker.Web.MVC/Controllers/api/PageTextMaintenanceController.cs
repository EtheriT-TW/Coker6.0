using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EtheriT.Coker.Web.MVC.Controllers.api
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class PageTextMaintenanceController : ControllerBase
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;

        public PageTextMaintenanceController(CokerDbContext db, LoginUserData loginUserData)
        {
            this.db = db;
            this.loginUserData = loginUserData;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatus()
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var states = await db.PageTextBackfillStates
                .AsNoTracking()
                .Where(x => x.FK_WebsiteId == websiteId)
                .OrderBy(x => x.ContentType)
                .Select(x => new
                {
                    x.ContentType,
                    x.Status,
                    x.TotalCount,
                    x.ProcessedCount,
                    Progress = x.TotalCount == 0 ? 100 : (int)Math.Floor(x.ProcessedCount * 100.0 / x.TotalCount),
                    x.FailedCount,
                    x.RemainingNullCount,
                    x.LastProcessedId,
                    x.TargetMaxId,
                    x.StartTime,
                    x.CompletionTime,
                    x.LastModificationTime,
                    x.LastError,
                    x.FailedIdsJson
                })
                .ToListAsync();

            var expectedTypes = new[]
            {
                PageTextBackfillJob.MenuType,
                PageTextBackfillJob.ArticleType,
                PageTextBackfillJob.ProductType
            };
            var pageTextReady = expectedTypes.All(type => states.Any(x =>
                x.ContentType == type
                && x.Status == "Completed"
                && x.FailedCount == 0
                && x.RemainingNullCount == 0));
            var fullText = await GetFullTextStatusAsync();
            var fullTextReady = fullText.IndexCount == 3 && fullText.PopulateStatus == 0;
            var ready = pageTextReady && fullTextReady;

            return Ok(new
            {
                WebsiteId = websiteId,
                PageTextReady = pageTextReady,
                FullTextIndexCount = fullText.IndexCount,
                FullTextPopulateStatus = fullText.PopulateStatus,
                FullTextReady = fullTextReady,
                ReadyForSearchSwitch = ready,
                Message = ready
                    ? "三類 PageText 已完成且沒有遺漏，可以進行搜尋切換。"
                    : "尚未完成；請等待離峰回填或處理失敗資料。",
                Items = states
            });
        }

        /// <summary>
        /// After the listed source data has been corrected, reset only failed
        /// content types. They will be rebuilt during the next off-peak run.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RetryFailed()
        {
            var websiteId = await loginUserData.GetWebsiteId();
            var states = await db.PageTextBackfillStates
                .Where(x => x.FK_WebsiteId == websiteId && x.Status == "CompletedWithErrors")
                .ToListAsync();
            foreach (var state in states)
            {
                state.Status = "Pending";
                state.LastProcessedId = 0;
                state.ProcessedCount = 0;
                state.FailedCount = 0;
                state.RemainingNullCount = 0;
                state.FailedIdsJson = null;
                state.LastError = null;
                state.StartTime = null;
                state.CompletionTime = null;
                state.LastModificationTime = DateTime.Now;
            }
            await db.SaveChangesAsync();
            return Ok(new
            {
                ResetCount = states.Count,
                Message = states.Count == 0
                    ? "目前沒有需要重試的 PageText 回填。"
                    : "失敗類型已重設，將於下一次離峰排程重新處理。"
            });
        }

        private async Task<(int IndexCount, int PopulateStatus)> GetFullTextStatusAsync()
        {
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose) await connection.OpenAsync();
            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT COUNT(*) AS IndexCount,
                           COALESCE(FULLTEXTCATALOGPROPERTY(N'CokerSearchCatalog', 'PopulateStatus'), -1) AS PopulateStatus
                    FROM sys.fulltext_indexes
                    WHERE object_id IN (
                        OBJECT_ID(N'[dbo].[WebMenus]'),
                        OBJECT_ID(N'[dbo].[Article]'),
                        OBJECT_ID(N'[dbo].[Prods]'))
                    """;
                await using var reader = await command.ExecuteReaderAsync();
                if (!await reader.ReadAsync()) return (0, -1);
                return (reader.GetInt32(0), reader.GetInt32(1));
            }
            finally
            {
                if (shouldClose) await connection.CloseAsync();
            }
        }
    }
}
