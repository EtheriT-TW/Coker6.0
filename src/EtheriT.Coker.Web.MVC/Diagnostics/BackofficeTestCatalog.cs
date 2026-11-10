using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public enum BackofficeTestStatus { NotRun, Passed, Failed, Skipped }

public sealed record BackofficeTestCase(
    string Id, string Title, string Steps, string Expected, string Mode)
{
    public BackofficeTestStatus Status { get; init; } = BackofficeTestStatus.NotRun;
    public List<string> RequiredFeatures { get; init; } = new();
    public string? Executor { get; init; }
    public string? ResultMessage { get; init; }
    public string StatusLabel => Status switch
    {
        BackofficeTestStatus.Passed => "通過",
        BackofficeTestStatus.Failed => "未通過",
        BackofficeTestStatus.Skipped => "未執行",
        _ => "尚未檢測"
    };

    public string EnvironmentLabel => Mode switch
    {
        "review" => "人工確認唯讀範圍",
        "isolated" => "隔離資料庫與檔案",
        "external" => "隔離環境＋外部服務沙盒",
        _ => "未定義，禁止執行"
    };
}

public sealed record BackofficeTestPage(
    string Id, string Module, string Controller, string Action, string Title,
    List<string> Sources, List<BackofficeTestCase> Cases)
{
    public BackofficeTestStatus BasicStatus { get; init; } = BackofficeTestStatus.NotRun;
    public string BasicMessage { get; init; } = "尚未檢測";
}

public sealed record BackofficeTestExclusion(string Controller, string Action, string Reason);

public sealed record BackofficeCatalogData(
    List<BackofficeTestPage> Pages, List<BackofficeTestExclusion> Exclusions);

public sealed record BackofficeCoverageGap(string Controller, string Action);

public sealed record BackofficeTestListModel(
    IReadOnlyList<BackofficeTestPage> Pages,
    IReadOnlyList<BackofficeCoverageGap> MissingPages,
    IReadOnlyList<BackofficeCoverageGap> StalePages,
    IReadOnlyList<BackofficeTestExclusion> Exclusions,
    int TotalCases, int PendingCases, int FailedCases, bool ShowAll, bool HasRun = false,
    int PlannedCases = 0)
{
    public string ScopeStamp => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
        System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Pages.Select(page => new
        {
            page.Id, page.Sources,
            Cases = page.Cases.Select(test => new { test.Id, test.Executor, test.Steps, test.Expected, test.RequiredFeatures })
        })))));
}

// Inventory only: no HTTP requests, controller execution, database or file mutation.
public static class BackofficeTestCatalog
{
    private static readonly Lazy<BackofficeCatalogData> Catalog = new(Load);

    public static BackofficeTestListModel Build(
        IActionDescriptorCollectionProvider descriptors, IEnumerable<JobMenu> scope, bool showAll)
    {
        var data = Catalog.Value;
        var enabledRoutes = new HashSet<BackofficeCoverageGap>();
        var enabledFeatures = new HashSet<string>(StringComparer.Ordinal);
        CollectEnabled(scope, enabledRoutes, enabledFeatures);
        // Login and session pages have no website feature switch.
        foreach (var route in new[] { new BackofficeCoverageGap("Account", "Index"),
            new BackofficeCoverageGap("Account", "Forget"), new BackofficeCoverageGap("Account", "NewPassword"),
            new BackofficeCoverageGap("Welcome", "Index") }) enabledRoutes.Add(route);

        var scoped = data.Pages
            .Where(page => enabledRoutes.Contains(new BackofficeCoverageGap(page.Controller, page.Action)))
            .Select(page => page with
            {
                Cases = page.Cases.Where(test => test.RequiredFeatures.All(enabledFeatures.Contains)).ToList()
            })
            .ToList();
        // Handwritten acceptance plans remain in the internal catalog. Only
        // implemented executors belong in the automatic checks shown to users.
        var plannedCases = scoped.Sum(page => page.Cases.Count(test => test.Executor == null));
        var applicable = scoped.Select(page => page with
        {
            Cases = page.Cases.Where(test => test.Executor != null).ToList()
        }).ToList();
        var actions = descriptors.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Where(action => action.ControllerTypeInfo.Namespace ==
                "EtheriT.Coker.Web.MVC.Controllers")
            .Select(action => new BackofficeCoverageGap(action.ControllerName, action.ActionName))
            .Distinct()
            .OrderBy(action => action.Controller).ThenBy(action => action.Action)
            .ToList();

        var registered = data.Pages.Select(page => new BackofficeCoverageGap(page.Controller, page.Action))
            .ToHashSet();
        var excluded = data.Exclusions.Select(page => new BackofficeCoverageGap(page.Controller, page.Action))
            .ToHashSet();
        var discovered = actions.ToHashSet();
        var missing = actions.Where(action => !registered.Contains(action) && !excluded.Contains(action)).ToList();
        var stale = registered.Where(action => !discovered.Contains(action))
            .OrderBy(action => action.Controller).ThenBy(action => action.Action).ToList();

        var pages = applicable.Select(page => page with
            {
                Cases = page.Cases.Where(test => showAll || test.Status == BackofficeTestStatus.Failed).ToList()
            }).Where(page => showAll || page.Cases.Count > 0).ToList();

        return new BackofficeTestListModel(
            pages, missing, stale, data.Exclusions,
            applicable.Sum(page => page.Cases.Count),
            applicable.Sum(page => page.Cases.Count(test => test.Status == BackofficeTestStatus.NotRun)),
            applicable.Sum(page => page.Cases.Count(test => test.Status == BackofficeTestStatus.Failed)), showAll,
            PlannedCases: plannedCases);
    }

    private static void CollectEnabled(IEnumerable<JobMenu> jobs,
        HashSet<BackofficeCoverageGap> routes, HashSet<string> features)
    {
        foreach (var job in jobs)
        {
            if (!job.Enable) continue;
            features.Add(job.PageName);
            if (!string.IsNullOrEmpty(job.Controller) && !string.IsNullOrEmpty(job.Action))
                routes.Add(new BackofficeCoverageGap(job.Controller, job.Action));
            if (job.jobItemModels != null) CollectEnabled(job.jobItemModels, routes, features);
        }
    }

    private static BackofficeCatalogData Load()
    {
        using var stream = typeof(BackofficeTestCatalog).Assembly
            .GetManifestResourceStream("Coker.Diagnostics.BackofficeCases.json")
            ?? throw new InvalidOperationException("Missing backoffice diagnostics catalog.");
        var data = JsonSerializer.Deserialize<BackofficeCatalogData>(stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Invalid backoffice diagnostics catalog.");

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var routes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var page in data.Pages)
        {
            if (string.IsNullOrWhiteSpace(page.Id) || !ids.Add(page.Id) ||
                page.BasicStatus != BackofficeTestStatus.NotRun ||
                string.IsNullOrWhiteSpace(page.Module) || string.IsNullOrWhiteSpace(page.Title) ||
                string.IsNullOrWhiteSpace(page.Controller) || string.IsNullOrWhiteSpace(page.Action) ||
                !routes.Add($"{page.Controller}/{page.Action}") ||
                page.Sources == null || page.Sources.Count == 0 ||
                page.Cases == null || page.Cases.Count == 0)
                throw new InvalidOperationException($"Invalid diagnostics page: {page.Id}");

            foreach (var test in page.Cases)
            {
                if (string.IsNullOrWhiteSpace(test.Id) || !ids.Add(test.Id) ||
                    test.Status != BackofficeTestStatus.NotRun || test.ResultMessage != null ||
                    string.IsNullOrWhiteSpace(test.Title) || string.IsNullOrWhiteSpace(test.Steps) ||
                    string.IsNullOrWhiteSpace(test.Expected) ||
                    test.RequiredFeatures == null || test.RequiredFeatures.Any(string.IsNullOrWhiteSpace) ||
                    test.Mode is not ("review" or "isolated" or "external"))
                    throw new InvalidOperationException($"Invalid diagnostics case: {test.Id}");
                if (test.Executor != null &&
                    !BackofficeReadOnlyChecks.Supports(page.Id, test.Executor) &&
                    !BackofficeObjectChecks.Supports(page.Id, test.Executor) &&
                    !BackofficeSandboxChecks.Supports(page.Id, test.Executor))
                    throw new InvalidOperationException($"Unknown diagnostics executor: {test.Id}");
            }
        }

        foreach (var exclusion in data.Exclusions)
        {
            if (string.IsNullOrWhiteSpace(exclusion.Controller) ||
                string.IsNullOrWhiteSpace(exclusion.Action) || string.IsNullOrWhiteSpace(exclusion.Reason) ||
                !routes.Add($"{exclusion.Controller}/{exclusion.Action}"))
                throw new InvalidOperationException("Invalid diagnostics exclusion.");
        }
        return data;
    }
}
