using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Common;
using Hangfire;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public sealed class DiagnosticsBackgroundJob(IServiceScopeFactory scopes, DiagnosticsWorkStore store,
    ILogger<DiagnosticsBackgroundJob> logger)
{
    [Queue("diagnostics")]
    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(3600)]
    public async Task ExecuteAsync(long websiteId, long userId, string runId, CancellationToken shutdown)
    {
        var work = store.Load(websiteId, userId);
        if (work == null || work.Id != runId || !work.Active) return;
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(shutdown);
        lifetime.CancelAfter(TimeSpan.FromMinutes(45));
        using var polling = new CancellationTokenSource();
        var watcher = WatchCancellation();
        await using var browserSession = new PublicBrowserSession();
        try
        {
            store.Update(websiteId, userId, runId, current => current with
            { State = "Running", Message = "背景檢測執行中，可離開頁面，稍後返回查看。" });
            for (var position = work.Completed; position < work.Targets.Count; position++)
            {
                lifetime.Token.ThrowIfCancellationRequested();
                var current = store.Load(websiteId, userId);
                if (current == null || current.Id != runId || !current.Active || current.CancelRequested)
                    throw new OperationCanceledException();
                // Each page gets a fresh context: no request cookies, stale tracked entities,
                // or client-selected URLs are carried into the background worker.
                await using var scope = scopes.CreateAsyncScope();
                var services = scope.ServiceProvider;
                services.GetRequiredService<BackgroundOperationContext>().Set(websiteId, userId);
                var login = services.GetRequiredService<LoginUserData>();
                if (!await login.IsPlatformAdministrator())
                    throw new InvalidOperationException("Diagnostics authorization was revoked.");
                var plans = services.GetRequiredService<DiagnosticsPlanBuilder>();
                using var pageTimeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
                pageTimeout.CancelAfter(TimeSpan.FromMinutes(3));
                var token = pageTimeout.Token;
                var index = work.Targets[position];
                PublicPageResult? publicResult = null;
                BackofficePageExecution? backofficeResult = null;
                if (work.PublicScan)
                {
                    var plan = await plans.PublicAsync(websiteId, token, work.FunctionScan);
                    if (plan.Error != null || plan.Stamp != work.Stamp || index < 0 || index >= plan.Targets.Count)
                        throw new InvalidOperationException("Diagnostics scope changed.");
                    publicResult = await PublicBrowserChecks.RunAsync(plan.Targets[index], plan.AllowedLinks,
                        services.GetRequiredService<IConfiguration>(), logger, token, work.FunctionScan, browserSession,
                        websiteId, plan.EnabledFeatures,
                        async (feature, cancellation) => await BackofficeSandboxChecks.ExecutePublicFeatureAsync(feature,
                            services.GetRequiredService<BackofficeSandboxFactory>(), services.GetRequiredService<AutoMapper.IMapper>(),
                            await login.GetWebsiteLevel(), cancellation),
                        work.FunctionScan ? async partial =>
                        {
                            using var progressConnection = store.Connect();
                            using var progressLock = store.Lock(progressConnection, websiteId, userId);
                            token.ThrowIfCancellationRequested();
                            var currentWork = store.Read(progressConnection, websiteId, userId);
                            if (currentWork == null || currentWork.Id != runId || !currentWork.Active || currentWork.CancelRequested)
                                throw new OperationCanceledException();
                            await new PublicResultStore(services.GetRequiredService<IWebHostEnvironment>().ContentRootPath, true)
                                .SaveAsync(websiteId, userId, work.Stamp, partial);
                            store.Save(progressConnection, websiteId, userId, currentWork);
                        } : null,
                        async cancellation => await BackofficeSandboxChecks.CreatePublicProductFixtureAsync(
                            services.GetRequiredService<BackofficeSandboxFactory>(), services.GetRequiredService<AutoMapper.IMapper>(),
                            await login.GetWebsiteLevel(), cancellation));
                }
                else
                {
                    var plan = await plans.BackofficeAsync();
                    if (plan.ScopeStamp != work.Stamp || index < 0 || index >= plan.Pages.Count)
                        throw new InvalidOperationException("Diagnostics scope changed.");
                    backofficeResult = await services.GetRequiredService<DiagnosticsPageRunner>()
                        .BackofficeAsync(websiteId, plan.Pages[index], token);
                }
                // Serialize result commits with enqueue/reset/stop. A deleted or stale
                // worker must never overwrite the next run's freshly reset results.
                using (var connection = store.Connect())
                using (store.Lock(connection, websiteId, userId))
                {
                    token.ThrowIfCancellationRequested();
                    var latest = store.Read(connection, websiteId, userId);
                    if (latest == null || latest.Id != runId || !latest.Active || latest.CancelRequested)
                        throw new OperationCanceledException();
                    var root = services.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
                    if (publicResult != null)
                        await new PublicResultStore(root, work.FunctionScan).SaveAsync(websiteId, userId, work.Stamp, publicResult);
                    else if (backofficeResult != null)
                        await new BackofficeResultStore(root).SaveAsync(websiteId, userId, work.Stamp,
                            new[] { backofficeResult.Page }, backofficeResult.Errors);
                    store.Save(connection, websiteId, userId, latest with { Completed = position + 1 });
                }
                if (publicResult?.StopRun == true) throw new InvalidOperationException("Browser diagnostics unavailable.");
            }
            store.Update(websiteId, userId, runId, latest => latest with
            { State = latest.CancelRequested ? "Stopped" : "Completed", Message = latest.CancelRequested
                ? "檢測已停止，已完成結果已儲存。" : "背景檢測完成，結果已儲存；未執行項目不代表通過。" });
        }
        catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
        {
            // Hangfire requeues shutdown-interrupted jobs; Completed allows resuming
            // after the last durably saved page, without relying on the browser page.
            throw;
        }
        catch (OperationCanceledException)
        {
            store.Update(websiteId, userId, runId, latest => latest with
            { State = latest.CancelRequested ? "Stopped" : "Interrupted", Message = latest.CancelRequested
                ? "檢測已停止，已完成結果仍保留。" : "檢測逾時中止，已完成結果仍保留，請重新檢測。" });
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Background diagnostics failed for site {Site}", websiteId);
            store.Update(websiteId, userId, runId, latest => latest with
            { State = "Interrupted", Message = "背景檢測中斷，請確認權限、功能範圍與檢測環境後重新檢測；已保存結果仍保留。" });
        }
        finally
        {
            polling.Cancel();
            await watcher;
        }

        async Task WatchCancellation()
        {
            try
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), polling.Token);
                    var latest = store.Load(websiteId, userId);
                    if (latest == null || latest.Id != runId || !latest.Active || latest.CancelRequested)
                    { lifetime.Cancel(); return; }
                }
            }
            catch (OperationCanceledException) when (polling.IsCancellationRequested) { }
            catch (Exception error)
            {
                logger.LogWarning(error, "Could not monitor diagnostics cancellation");
                lifetime.Cancel(); // Fail closed if work ownership/cancellation cannot be checked.
            }
        }
    }
}
