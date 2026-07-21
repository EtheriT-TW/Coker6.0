using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EtheriT.Coker.Application.BackgroundJob
{
    /// <summary>
    /// Rebuilds PageText in small resumable batches. Search is intentionally not
    /// switched by this job; the status API is the release gate for that change.
    /// </summary>
    public sealed class PageTextBackfillJob
    {
        public const string MenuType = "WebMenu";
        public const string ArticleType = "Article";
        public const string ProductType = "Prod";

        private const int BatchSize = 200;
        private static readonly TimeSpan RunBudget = TimeSpan.FromMinutes(90);

        private readonly CokerDbContext db;
        private readonly StringHandler stringHandler;
        private readonly IHtmlProcessor htmlProcessor;
        private readonly ILogger<PageTextBackfillJob> logger;

        public PageTextBackfillJob(
            CokerDbContext db,
            StringHandler stringHandler,
            IHtmlProcessor htmlProcessor,
            ILogger<PageTextBackfillJob> logger)
        {
            this.db = db;
            this.stringHandler = stringHandler;
            this.htmlProcessor = htmlProcessor;
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(7200)]
        public async Task Run()
        {
            var deadline = DateTime.UtcNow.Add(RunBudget);
            var websiteIds = await db.Websites
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.Id)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var websiteId in websiteIds)
                await EnsureStatesAsync(websiteId);

            while (DateTime.UtcNow < deadline)
            {
                var states = await db.PageTextBackfillStates
                    .Where(x => x.Status != "Completed" && x.Status != "CompletedWithErrors")
                    .OrderBy(x => x.LastModificationTime)
                    .ThenBy(x => x.FK_WebsiteId)
                    .ThenBy(x => x.ContentType)
                    .ToListAsync();

                if (states.Count == 0) break;

                var madeProgress = false;
                foreach (var state in states)
                {
                    if (DateTime.UtcNow >= deadline) break;
                    madeProgress |= await ProcessBatchAsync(state);
                }

                if (!madeProgress) break;
            }
        }

        private async Task EnsureStatesAsync(long websiteId)
        {
            await EnsureStateAsync(websiteId, MenuType,
                db.WebMenus.Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted));
            await EnsureStateAsync(websiteId, ArticleType,
                db.Article.Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted));
            await EnsureStateAsync(websiteId, ProductType,
                db.Prods.Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted));
        }

        private async Task EnsureStateAsync<TEntity>(
            long websiteId,
            string contentType,
            IQueryable<TEntity> query) where TEntity : EtheriT.Coker.Core.Entity.FullAuditedEntity
        {
            if (await db.PageTextBackfillStates.AnyAsync(x =>
                    x.FK_WebsiteId == websiteId && x.ContentType == contentType))
                return;

            var targetMaxId = await query.Select(x => (long?)x.Id).MaxAsync() ?? 0;
            var totalCount = targetMaxId == 0
                ? 0
                : await query.CountAsync(x => x.Id <= targetMaxId);

            db.PageTextBackfillStates.Add(new PageTextBackfillState
            {
                FK_WebsiteId = websiteId,
                ContentType = contentType,
                Status = totalCount == 0 ? "Completed" : "Pending",
                TargetMaxId = targetMaxId,
                TotalCount = totalCount,
                CompletionTime = totalCount == 0 ? DateTime.Now : null,
                CreationTime = DateTime.Now
            });
            await db.SaveChangesAsync();
        }

        private async Task<bool> ProcessBatchAsync(PageTextBackfillState state)
        {
            state.Status = "Running";
            state.StartTime ??= DateTime.Now;
            state.LastModificationTime = DateTime.Now;

            var rows = state.ContentType switch
            {
                MenuType => await db.WebMenus.AsNoTracking()
                    .Where(x => x.FK_WebsiteId == state.FK_WebsiteId && !x.IsDeleted)
                    .Where(x => x.Id > state.LastProcessedId && x.Id <= state.TargetMaxId)
                    .OrderBy(x => x.Id)
                    .Select(x => new PageTextSourceRow(x.Id, x.Html, x.LastModificationTime))
                    .Take(BatchSize).ToListAsync(),
                ArticleType => await db.Article.AsNoTracking()
                    .Where(x => x.FK_WebsiteId == state.FK_WebsiteId && !x.IsDeleted)
                    .Where(x => x.Id > state.LastProcessedId && x.Id <= state.TargetMaxId)
                    .OrderBy(x => x.Id)
                    .Select(x => new PageTextSourceRow(x.Id, x.Html, x.LastModificationTime))
                    .Take(BatchSize).ToListAsync(),
                ProductType => await db.Prods.AsNoTracking()
                    .Where(x => x.FK_WebsiteId == state.FK_WebsiteId && !x.IsDeleted)
                    .Where(x => x.Id > state.LastProcessedId && x.Id <= state.TargetMaxId)
                    .OrderBy(x => x.Id)
                    .Select(x => new PageTextSourceRow(x.Id, x.Html, x.LastModificationTime))
                    .Take(BatchSize).ToListAsync(),
                _ => throw new InvalidOperationException($"Unknown PageText content type: {state.ContentType}")
            };

            if (rows.Count == 0)
            {
                await CompleteStateAsync(state);
                return false;
            }

            var failedIds = string.IsNullOrWhiteSpace(state.FailedIdsJson)
                ? new List<long>()
                : JsonConvert.DeserializeObject<List<long>>(state.FailedIdsJson!) ?? new List<long>();

            foreach (var row in rows)
            {
                try
                {
                    var pageText = htmlProcessor.text(stringHandler.HtmlDecode(row.Html ?? string.Empty)) ?? string.Empty;
                    await UpdatePageTextAsync(state.ContentType, row, pageText);
                }
                catch (Exception ex)
                {
                    state.FailedCount++;
                    if (failedIds.Count < 1000) failedIds.Add(row.Id);
                    state.LastError = ex.Message.Length <= 4000 ? ex.Message : ex.Message[..4000];
                    logger.LogWarning(ex,
                        "PageText backfill failed. WebsiteId={WebsiteId}, ContentType={ContentType}, ContentId={ContentId}",
                        state.FK_WebsiteId, state.ContentType, row.Id);
                }
            }

            state.LastProcessedId = rows[^1].Id;
            state.ProcessedCount = Math.Min(state.TotalCount, state.ProcessedCount + rows.Count);
            state.FailedIdsJson = failedIds.Count == 0 ? null : JsonConvert.SerializeObject(failedIds);
            state.LastModificationTime = DateTime.Now;
            await db.SaveChangesAsync();
            return true;
        }

        private Task<int> UpdatePageTextAsync(string contentType, PageTextSourceRow row, string pageText)
        {
            return contentType switch
            {
                MenuType => db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE [WebMenus] SET [PageText] = {pageText} WHERE [Id] = {row.Id} AND (([LastModificationTime] = {row.LastModificationTime}) OR ([LastModificationTime] IS NULL AND {row.LastModificationTime} IS NULL))"),
                ArticleType => db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE [Article] SET [PageText] = {pageText} WHERE [Id] = {row.Id} AND (([LastModificationTime] = {row.LastModificationTime}) OR ([LastModificationTime] IS NULL AND {row.LastModificationTime} IS NULL))"),
                ProductType => db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE [Prods] SET [PageText] = {pageText} WHERE [Id] = {row.Id} AND (([LastModificationTime] = {row.LastModificationTime}) OR ([LastModificationTime] IS NULL AND {row.LastModificationTime} IS NULL))"),
                _ => throw new InvalidOperationException($"Unknown PageText content type: {contentType}")
            };
        }

        private async Task CompleteStateAsync(PageTextBackfillState state)
        {
            state.RemainingNullCount = state.ContentType switch
            {
                MenuType => await db.WebMenus.CountAsync(x => x.FK_WebsiteId == state.FK_WebsiteId
                    && !x.IsDeleted && x.Id <= state.TargetMaxId && x.PageText == null),
                ArticleType => await db.Article.CountAsync(x => x.FK_WebsiteId == state.FK_WebsiteId
                    && !x.IsDeleted && x.Id <= state.TargetMaxId && x.PageText == null),
                ProductType => await db.Prods.CountAsync(x => x.FK_WebsiteId == state.FK_WebsiteId
                    && !x.IsDeleted && x.Id <= state.TargetMaxId && x.PageText == null),
                _ => 1
            };
            state.Status = state.FailedCount == 0 && state.RemainingNullCount == 0
                ? "Completed"
                : "CompletedWithErrors";
            state.CompletionTime = DateTime.Now;
            state.LastModificationTime = DateTime.Now;
            await db.SaveChangesAsync();
        }

        private sealed record PageTextSourceRow(long Id, string? Html, DateTime? LastModificationTime);
    }
}
