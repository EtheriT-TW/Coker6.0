using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.SiteCrawl;
using EtheriT.Coker.Core.Models;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Application.BackgroundJob;

public sealed class SiteCrawlBackgroundJob(
    BackgroundTaskService tasks,
    BackgroundOperationContext operationContext,
    LoginUserData user,
    SiteMenuImporter importer,
    SiteLayoutImporter layoutImporter,
    SiteContentImporter contentImporter,
    SiteArticleImporter articleImporter,
    SiteGlobalCssImporter globalCssImporter,
    SiteMarqueeImporter marqueeImporter,
    SiteListDetector listDetector,
    IOptions<SiteCrawlOptions> options,
    ILogger<SiteCrawlBackgroundJob> logger)
{
    public const string EmptyResult = """{"menusCreated":0,"articlesCreated":0,"errors":[]}""";

    // productLists：匯入前偵測、使用者確認要當商品的列表（SiteListDetector.ListKey），這次先不匯入。
    [AutomaticRetry(Attempts = 0)]
    public async Task Run(long taskId, string sourceUrl, string[] productLists, CancellationToken cancellationToken)
    {
        if (await BeginAsync(taskId) is not { } started) return;
        using (started.Lock)
            await ImportMenusAsync(started.Task, sourceUrl, productLists, cancellationToken);
    }

    // 匯入前的列表偵測：只用 HTTP 掃描，找出列表並預猜文章／商品，結果放在 ResultJson 給確認畫面用。
    [AutomaticRetry(Attempts = 0)]
    public async Task Detect(long taskId, string sourceUrl, CancellationToken cancellationToken)
    {
        if (await BeginAsync(taskId) is not { } started) return;
        using (started.Lock)
            await DetectListsAsync(started.Task, sourceUrl, cancellationToken);
    }

    private async Task<(BackgroundTaskRecord Task, IDisposable Lock)?> BeginAsync(long taskId)
    {
        var task = await tasks.GetAsync(taskId);
        if (task is null || task.Type != BackgroundTaskTypeEnum.SiteCrawl || task.ActiveKey is null)
            return null;

        operationContext.Set(task.FK_WebsiteId, task.FK_UserId);
        var connection = JobStorage.Current.GetConnection();
        var operationLock = connection.AcquireDistributedLock(
            BackgroundTaskService.SiteCrawlKey(task.FK_WebsiteId), TimeSpan.FromSeconds(5));
        var held = new CompositeLock(operationLock, connection);
        if (await tasks.StartSiteCrawlAsync(task)) return (task, held);
        held.Dispose();
        await FinishCancellationAsync(task);
        return null;
    }

    private sealed class CompositeLock(IDisposable operationLock, IDisposable connection) : IDisposable
    {
        public void Dispose()
        {
            operationLock.Dispose();
            connection.Dispose();
        }
    }

    private async Task DetectListsAsync(BackgroundTaskRecord task, string sourceUrl, CancellationToken cancellationToken)
    {
        var result = new SiteDetectResult();
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (options.Value.MaxSeconds > 0)
            timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.MaxSeconds));

        async Task Checkpoint(int pages)
        {
            if (!await tasks.UpdateSiteCrawlProgressAsync(task, Math.Min(90, 10 + pages / 2), $"正在偵測列表，已讀取 {pages} 頁"))
                throw new OperationCanceledException("網站匯入任務已停止。");
            timeout.Token.ThrowIfCancellationRequested();
        }

        try
        {
            if (!await user.isSystemUser() || !await user.CheckedWebSiteId(task.FK_UserId, task.FK_WebsiteId))
                throw new UnauthorizedAccessException("網站匯入權限已撤銷。");
            await listDetector.DetectAsync(new Uri(sourceUrl), result, Checkpoint, timeout.Token);
            var message = $"偵測完成：讀取 {result.PagesScanned} 頁，找到 {result.Lists.Count} 個列表，請確認後開始匯入。";
            if (!await tasks.FinishSiteCrawlAsync(task, BackgroundTaskStatusEnum.Running,
                BackgroundTaskStatusEnum.Succeeded, message, result.Serialize()))
                await FinishCancellationAsync(task, result.Serialize());
        }
        catch (Exception ex)
        {
            var summary = result.Serialize();
            if (await FinishCancellationAsync(task, summary)) return;
            logger.LogError(ex, "Site crawl list detection failed for task {TaskId}", task.Id);
            var error = ex switch
            {
                OperationCanceledException when timeout.IsCancellationRequested => "列表偵測超過時間上限。",
                UnauthorizedAccessException => "網站匯入權限已撤銷。",
                _ => $"無法完成列表偵測，請查看 MVC 日誌中的任務 {task.Id}。"
            };
            if (!await tasks.FinishSiteCrawlAsync(task, BackgroundTaskStatusEnum.Running,
                BackgroundTaskStatusEnum.Failed, "列表偵測失敗", summary, error))
                throw;
        }
    }

    private async Task ImportMenusAsync(BackgroundTaskRecord task, string sourceUrl, string[] productLists,
        CancellationToken cancellationToken)
    {
        var result = new SiteLayoutResult();
        result.ProductListKeys.UnionWith(productLists);
        var scanningLayout = false;
        var importingContent = false;
        var importingArticles = false;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (options.Value.MaxSeconds > 0)
            timeout.CancelAfter(TimeSpan.FromSeconds(options.Value.MaxSeconds));

        async Task Checkpoint()
        {
            var processed = result.MenusCreated + result.MenusUpdated;
            var progress = result.ScanComplete ? Math.Min(95, 30 + processed) : 10;
            var message = result.ScanComplete ? $"正在匯入選單，已處理 {processed} 個" : "正在抓取入口頁的導覽選單";
            if (scanningLayout)
            {
                progress = 95;
                message = $"正在處理版頭與頁尾，已掃描 {result.PagesScanned} 頁，其中 {result.PagesRendered} 頁由瀏覽器讀取";
            }
            if (importingArticles)
            {
                progress = 97;
                message = $"正在匯入文章，已補抓 {result.ArticlesFetched} 篇詳細頁、完成 {result.ArticlesCreated + result.ArticlesUpdated} 篇";
            }
            if (importingContent)
            {
                progress = 98;
                message = $"正在匯入頁面內容，已完成 {result.PagesImported} 頁";
            }
            if (!await tasks.UpdateSiteCrawlProgressAsync(task, progress, message))
                throw new OperationCanceledException("網站匯入任務已停止。");
            timeout.Token.ThrowIfCancellationRequested();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await user.isSystemUser() || !await user.CheckedWebSiteId(task.FK_UserId, task.FK_WebsiteId))
                throw new UnauthorizedAccessException("網站匯入權限已撤銷。");
            result.OrgName = await user.GetWebsiteOrgName(task.FK_WebsiteId);

            var entry = await importer.ImportAsync(new Uri(sourceUrl), task.FK_WebsiteId, result, Checkpoint, timeout.Token);
            await Checkpoint();
            if (result.MenusCreated + result.MenusUpdated == 0)
                throw new InvalidOperationException("沒有儲存任何選單。");
            scanningLayout = true;
            result.PagesScanned = 0;
            result.PagesRendered = 0;
            var layout = await layoutImporter.ImportAsync(entry, result, Checkpoint, timeout.Token);
            await Checkpoint();
            await importer.ImportSectionMenusAsync(layout, task.FK_WebsiteId, result, Checkpoint, timeout.Token);
            await importer.ImportHiddenPagesAsync(layout, task.FK_WebsiteId, result, Checkpoint, timeout.Token);
            await Checkpoint();
            importingArticles = true;
            var listMenuIds = await articleImporter.ImportAsync(layout, result, Checkpoint, timeout.Token);
            await Checkpoint();
            importingArticles = false;
            importingContent = true;
            listMenuIds.UnionWith(result.HandledMenuIds);
            await contentImporter.ImportAsync(layout, listMenuIds, result, Checkpoint, timeout.Token);
            await Checkpoint();
            await globalCssImporter.ImportAsync(entry, layout, result, Checkpoint, timeout.Token);
            await Checkpoint();
            await marqueeImporter.ImportAsync(layout, result, Checkpoint, timeout.Token);
            await Checkpoint();
            var message = result.ErrorCount > 0 ? "選單匯入完成，部分項目未能處理。" : "網站匯入完成。";
            if (!await tasks.FinishSiteCrawlAsync(task, BackgroundTaskStatusEnum.Running,
                BackgroundTaskStatusEnum.Succeeded, message, result.Serialize()))
                await FinishCancellationAsync(task, result.Serialize());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (!await FinishCancellationAsync(task, result.Serialize()))
                throw;
        }
        catch (Exception ex)
        {
            var summary = result.Serialize();
            if (await FinishCancellationAsync(task, summary)) return;
            logger.LogError(ex, "Site crawl failed for task {TaskId} after scanning {PagesScanned} pages",
                task.Id, result.PagesScanned);
            var error = ex switch
            {
                OperationCanceledException when timeout.IsCancellationRequested => "網站匯入超過時間上限。",
                UnauthorizedAccessException => "網站匯入權限已撤銷。",
                _ when result.MenusCreated + result.MenusUpdated == 0 && result.Errors.Count > 0 => result.Errors[0],
                _ => $"無法完成網站匯入任務，請查看 MVC 日誌中的任務 {task.Id}。"
            };
            if (!await tasks.FinishSiteCrawlAsync(task, BackgroundTaskStatusEnum.Running,
                BackgroundTaskStatusEnum.Failed, "網站匯入任務失敗", summary, error)
                && await FinishCancellationAsync(task, summary))
                return;
            throw;
        }
    }

    private Task<bool> FinishCancellationAsync(BackgroundTaskRecord task, string summary = EmptyResult)
        => tasks.FinishSiteCrawlAsync(task, BackgroundTaskStatusEnum.CancelRequested,
            BackgroundTaskStatusEnum.Canceled, "網站匯入已取消", summary);
}
