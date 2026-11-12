using AutoMapper;
using EtheriT.Coker.Application;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Newtonsoft.Json.Linq;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public sealed record BackofficePageExecution(BackofficeTestPage Page, Dictionary<string, string> Errors);

public sealed class DiagnosticsPageRunner(IServiceProvider services, LoginUserData loginUserData,
    IActionDescriptorCollectionProvider actionDescriptors, IRazorViewEngine viewEngine,
    BackofficeSandboxFactory sandboxFactory, IMapper mapper,
    ILogger<DiagnosticsPageRunner> logger)
{
    public async Task<BackofficePageExecution> BackofficeAsync(long websiteId,
        BackofficeTestPage selected, CancellationToken token)
    {
        var level = await loginUserData.GetWebsiteLevel();
        var actions = actionDescriptors.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Select(action => (action.ControllerName, action.ActionName)).ToHashSet();
        var pages = new List<BackofficeTestPage>();
        var errors = new Dictionary<string, string>();
        foreach (var page in new[] { selected })
        {
            var passed = false;
            try
            {
                var views = page.Sources.Where(source => source.StartsWith("Views/", StringComparison.Ordinal)
                    && source.EndsWith(".cshtml", StringComparison.Ordinal)).ToList();
                passed = actions.Contains((page.Controller, page.Action)) && views.Count > 0 &&
                    views.All(view => viewEngine.GetView(null, "~/" + view, true).Success);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Diagnostics view lookup failed for {PageId}", page.Id);
                errors[page.Id] = exception.ToString();
            }
            var cases = new List<BackofficeTestCase>();
            var samples = new Dictionary<string, JArray>();
            foreach (var test in page.Cases)
            {
                token.ThrowIfCancellationRequested();
                var status = BackofficeTestStatus.Skipped;
                var message = "尚未實作此項自動檢測，需另行驗證。";
                try
                {
                    var check = await BackofficeReadOnlyChecks.ExecuteAsync(page.Id, test.Executor,
                        services, token, samples);
                    check ??= await BackofficeObjectChecks.ExecuteAsync(page.Id, test.Executor,
                        services, token);
                    check ??= await BackofficeSandboxChecks.ExecuteAsync(page.Id, test.Executor,
                        sandboxFactory, mapper, level, token);
                    if (check != null)
                    {
                        status = check.Status;
                        message = check.Message;
                        if (status == BackofficeTestStatus.Failed)
                        {
                            errors[test.Id] = message;
                            logger.LogWarning("Diagnostics assertion failed for {CaseId} on site {WebsiteId}: {Summary}",
                                test.Id, websiteId, message);
                        }
                    }
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    logger.LogWarning(exception, "Diagnostics failed for {CaseId} on site {WebsiteId}", test.Id, websiteId);
                    errors[test.Id] = exception.ToString();
                    status = BackofficeTestStatus.Failed;
                    message = "此項檢測未通過，請聯絡系統維護人員。";
                }
                finally
                {
                    // A blocked SaveChanges must not leave mutated tracked
                    // entities available to a later check in this request.
                    services.GetRequiredService<CokerDbContext>().ChangeTracker.Clear();
                }
                cases.Add(test with { Status = status, ResultMessage = message });
            }
            pages.Add(page with
            {
                BasicStatus = passed ? BackofficeTestStatus.Passed : BackofficeTestStatus.Failed,
                BasicMessage = passed ? "頁面基本檢查通過；完整操作尚未驗證。" : "頁面基本檢查未通過，請聯絡系統維護人員。",
                Cases = cases
            });
        }
        token.ThrowIfCancellationRequested();
        return new BackofficePageExecution(pages.Single(), errors);
    }
}
