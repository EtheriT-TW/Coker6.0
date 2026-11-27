using System.Globalization;
using System.Net;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EtheriT.Coker.Application.SiteCrawl;

public sealed record SiteCrawlRemovalItem(string Id, string Kind, string Title, string? SourceUrl, long NewId, string? DesktopImage);
public sealed record SiteCrawlRemovalBatch(long TaskId, SiteCrawlRemovalItem[] Removed, SiteCrawlRemovalItem[] NotScanned);
public sealed record SiteCrawlRemovalSummary(string Id, string Kind, string Title, string? SourceUrl);

public sealed class SiteCrawlRemovalService(CokerDbContext db, LoginUserData user, SiteCrawlHttp http,
    SiteCrawlPendingStore pending, BackgroundTaskService tasks, IWebsiteCacheStateAppService cache,
    ILogger<SiteCrawlRemovalService> logger)
{
    internal async Task DetectAsync(Uri sourceRoot, SiteLayoutSelection layout, SiteLayoutResult result, Func<Task> checkpoint,
        CancellationToken token)
    {
        if (result.ImportState is not { } state || result.OrgName is not { } org) return;
        var excluded = result.ExcludedPageKeys.Select(WithoutWww).ToHashSet(StringComparer.Ordinal);
        var seenMenus = layout.Pages.Select(x => state.MenuKey(x.Url)).ToHashSet(StringComparer.Ordinal);
        var seenDetails = layout.Pages.Where(x => x.Article != null)
            .Select(x => SiteCrawlHtml.DetailKey(x.Url)).ToHashSet(StringComparer.Ordinal);
        var excludedDetails = layout.Pages.Where(x => IsExcluded(x.Url))
            .SelectMany(x => x.List?.Items ?? []).Select(x => SiteCrawlHtml.DetailKey(x.Url)).ToHashSet(StringComparer.Ordinal);
        var timedOutMenus = result.TimedOutSources.Select(state.MenuKey).ToHashSet(StringComparer.Ordinal);
        var timedOutDetails = result.TimedOutSources.Select(SiteCrawlHtml.DetailKey).ToHashSet(StringComparer.Ordinal);
        var menuClaims = result.ImportedMenus.Where(x => !x.IsAlias && Uri.TryCreate(x.SourceUrl, UriKind.Absolute, out _))
            .ToLookup(x => x.Id, x => state.MenuKey(new Uri(x.SourceUrl!)));
        var seenSliders = state.Items.Where(pair => result.SeenSliderKeys.Contains(pair.Key) && pair.Value.DesktopImage != null)
            .Select(pair => (pair.Value.NewId, pair.Value.DesktopImage!.ToUpperInvariant())).ToHashSet();
        var checkedUrls = new Dictionary<string, HttpStatusCode?>(StringComparer.Ordinal);
        var targets = new Dictionary<(string Kind, long Id, string? Image), (SiteCrawlRemovalItem Item, bool Deleted)>();
        var protectedPages = new HashSet<(string Kind, long Id)>();
        foreach (var (key, item) in state.Items)
        {
            var kind = key.Split('|', 2)[0];
            if (kind is not ("Menu" or "Article" or "Product")) continue;
            if (!Uri.TryCreate(item.SourceUrl, UriKind.Absolute, out var url) || IsExcluded(url)
                || (kind == "Menu" ? seenMenus.Contains(key) || timedOutMenus.Contains(key)
                    : IsProtectedDetail(url, item.SourceListUrl)))
                protectedPages.Add((kind, item.NewId));
        }

        foreach (var (key, item) in state.Items)
        {
            var kind = key.Split('|', 2)[0];
            Uri.TryCreate(item.SourceUrl, UriKind.Absolute, out var source);
            bool isRemoved;
            switch (kind)
            {
                case "Menu":
                    if (source == null || protectedPages.Contains((kind, item.NewId))
                        || menuClaims[item.NewId].Any(claim => claim != key)) continue;
                    isRemoved = await ProbeAsync(source);
                    break;
                case "Article":
                case "Product":
                    if (source == null || protectedPages.Contains((kind, item.NewId))
                        || result.ClaimedRemovalTargets.Contains((kind, item.NewId))) continue;
                    isRemoved = await ProbeAsync(source);
                    break;
                case "Marquee":
                    if (result.SeenMarqueeKeys.Contains(key) || result.ClaimedRemovalTargets.Contains((kind, item.NewId))) continue;
                    isRemoved = IsSectionRemoval(result.SeenMarqueeKeys.Count, false);
                    break;
                case "Slider":
                    if (result.SeenSliderKeys.Contains(key) || string.IsNullOrEmpty(item.DesktopImage)) continue;
                    var slider = (item.NewId, item.DesktopImage.ToUpperInvariant());
                    if (seenSliders.Contains(slider) || result.ClaimedSliders.Contains(slider)) continue;
                    isRemoved = IsSectionRemoval(result.SliderSectionSeen ? 1 : 0, false);
                    break;
                default:
                    continue;
            }
            var target = (kind, item.NewId, item.DesktopImage?.ToUpperInvariant());
            // 多個舊網址對到同一筆資料時，只要有一個網址尚無法確認移除，就不提供下架。
            if (targets.TryGetValue(target, out var prior) && (!prior.Deleted || isRemoved)) continue;
            var title = item.SourceTitle ?? item.WrittenTitle ?? source?.PathAndQuery ?? key;
            targets[target] = (new("", kind, title, item.SourceUrl, item.NewId, item.DesktopImage), isRemoved);
        }
        if (targets.Count == 0)
        {
            result.Removed = 0;
            result.NotScanned = 0;
            return;
        }
        var rows = targets.Values.Select((x, index) => (Item: x.Item with { Id = (index + 1).ToString(CultureInfo.InvariantCulture) },
            x.Deleted)).ToArray();
        var removed = rows.Where(x => x.Deleted).Select(x => x.Item).ToArray();
        var notScanned = rows.Where(x => !x.Deleted).Select(x => x.Item).ToArray();
        try
        {
            await pending.WriteRemovalsAsync(org, new(result.ImportTaskId, removed, notScanned));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Could not save removal review for site crawl task {TaskId}", result.ImportTaskId);
            result.AddError("無法儲存舊站刪除檢查結果。");
            return;
        }
        result.Removed = removed.Length;
        result.NotScanned = notScanned.Length;

        bool IsProtectedDetail(Uri source, string? listUrl)
        {
            var detail = SiteCrawlHtml.DetailKey(source);
            return seenDetails.Contains(detail) || excludedDetails.Contains(detail) || timedOutDetails.Contains(detail)
                || (Uri.TryCreate(listUrl, UriKind.Absolute, out var list) && IsExcluded(list));
        }

        bool IsExcluded(Uri url) => excluded.Contains(WithoutWww(SiteLayoutScanner.PageKey(url)))
            || excluded.Contains(WithoutWww(SiteListDetector.ListKey(url)));

        async Task<bool> ProbeAsync(Uri source)
        {
            if (!SiteCrawlHttp.IsAllowed(source, source.IdnHost)
                || SiteCrawlStateStore.NormalizeHost(source.IdnHost) != SiteCrawlStateStore.NormalizeHost(sourceRoot.IdnHost)) return false;
            if (!checkedUrls.TryGetValue(source.AbsoluteUri, out var status))
            {
                await checkpoint();
                status = await http.ProbeStatusAsync(source, token);
                checkedUrls.Add(source.AbsoluteUri, status);
            }
            return IsDeletedStatus(status);
        }
    }

    private static string WithoutWww(string key) => key.StartsWith("www.", StringComparison.Ordinal) ? key[4..] : key;

    public static bool IsDeletedStatus(HttpStatusCode? status) => status is HttpStatusCode.NotFound or HttpStatusCode.Gone;
    public static bool IsSectionRemoval(int scannedCount, bool seen) => scannedCount > 0 && !seen;

    public async Task<(SiteCrawlRemovalSummary[] Removed, SiteCrawlRemovalSummary[] NotScanned)> ListAsync(
        long taskId, CancellationToken token)
    {
        var org = await user.GetWebsiteOrgName(await user.GetWebsiteId());
        var batch = await pending.ReadRemovalsAsync(org, taskId, token);
        return (Summaries(batch.Removed), Summaries(batch.NotScanned));
    }

    private static SiteCrawlRemovalSummary[] Summaries(SiteCrawlRemovalItem[] items) =>
        items.Select(x => new SiteCrawlRemovalSummary(x.Id, x.Kind, x.Title, x.SourceUrl)).ToArray();

    public async Task<IReadOnlyList<SiteCrawlConflictOutcome>> UnpublishAsync(long taskId, IReadOnlySet<string> selected,
        CancellationToken token)
    {
        var site = await user.GetWebsiteId();
        var org = await user.GetWebsiteOrgName(site);
        using var connection = JobStorage.Current.GetConnection();
        using var operationLock = SiteCrawlConflictService.AcquireLock(connection, site);
        if (await tasks.HasActiveSiteCrawlAsync(site, token))
            throw new BackgroundTaskConflictException("網站匯入任務處理中，請等任務完成後再下架。");
        var batch = await pending.ReadRemovalsAsync(org, taskId, token);
        if (!selected.IsSubsetOf(batch.Removed.Select(x => x.Id)))
            throw new BackgroundTaskConflictException("這批下架項目已變更，請重新載入匯入結果。");

        var outcomes = new List<SiteCrawlConflictOutcome>();
        var done = new HashSet<string>(StringComparer.Ordinal);
        try
        {
            foreach (var item in batch.Removed.Where(x => selected.Contains(x.Id)))
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    var applied = await UnpublishItemAsync(site, item, token);
                    if (applied) done.Add(item.Id);
                    outcomes.Add(new(item.Id, item.Title, applied ? "unpublished" : "missing"));
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    db.ChangeTracker.Clear();
                    logger.LogWarning(ex, "Could not unpublish imported item {Kind} {Id} for site {Site}", item.Kind, item.NewId, site);
                    outcomes.Add(new(item.Id, item.Title, "failed"));
                }
            }
        }
        finally
        {
            if (done.Count > 0)
                await pending.WriteRemovalsAsync(org, batch with { Removed = batch.Removed.Where(x => !done.Contains(x.Id)).ToArray() });
        }
        return outcomes;
    }

    private async Task<bool> UnpublishItemAsync(long site, SiteCrawlRemovalItem item, CancellationToken token)
    {
        var id = item.NewId;
        switch (item.Kind)
        {
            case "Menu":
                var menu = await db.WebMenus.SingleOrDefaultAsync(x => x.Id == id && x.FK_WebsiteId == site && !x.IsDeleted, token);
                if (menu == null) return false;
                if (!menu.RemovedFromShelves)
                {
                    menu.RemovedFromShelves = true;
                    await user.SaveChanges(menu);
                }
                await cache.TouchByWebsiteIdAsync(site, WebsiteCacheKeys.Menu);
                await cache.TouchByWebsiteIdAsync(site, WebsiteCacheKeys.DirectoryMenu);
                return true;
            case "Article":
                var article = await db.Article.SingleOrDefaultAsync(x => x.Id == id && x.FK_WebsiteId == site && !x.IsDeleted, token);
                if (article == null) return false;
                if (!article.RemovedFromShelves)
                {
                    article.RemovedFromShelves = true;
                    await user.SaveChanges(article);
                }
                await cache.TouchByWebsiteIdAsync(site, WebsiteCacheKeys.DirectoryContent);
                return true;
            case "Product":
                var product = await db.Prods.SingleOrDefaultAsync(x => x.Id == id && x.FK_WebsiteId == site && !x.IsDeleted, token);
                if (product == null) return false;
                if (!product.RemovedFromShelves)
                {
                    product.RemovedFromShelves = true;
                    await user.SaveChanges(product);
                }
                await cache.TouchByWebsiteIdAsync(site, WebsiteCacheKeys.DirectoryContent);
                return true;
            case "Marquee":
                var marquee = await db.Marquees.SingleOrDefaultAsync(x => x.Id == id && x.FK_WebsiteId == site && !x.IsDeleted, token);
                if (marquee == null) return false;
                if (marquee.disp_opt)
                {
                    marquee.disp_opt = false;
                    await user.SaveChanges(marquee);
                }
                return true;
            case "Slider":
                var header = await db.TemplateSections.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted
                    && x.sectionType == SectionTypeEnum.表頭 && !x.template.IsDeleted && x.template.FK_WebsiteID == site, token);
                if (header == null || string.IsNullOrWhiteSpace(header.ContentConfig) || item.DesktopImage == null) return false;
                var config = JObject.Parse(header.ContentConfig);
                if (!DisableSlider(config, item.DesktopImage)) return false;
                var updatedConfig = config.ToString(Formatting.None);
                if (header.ContentConfig != updatedConfig)
                {
                    header.ContentConfig = updatedConfig;
                    await user.SaveChanges(header);
                }
                return true;
            default:
                return false;
        }
    }

    public static bool DisableSlider(JObject config, string desktopImage)
    {
        if (config.GetValue("Sliders", StringComparison.OrdinalIgnoreCase) is not JArray sliders) return false;
        var matches = sliders.OfType<JObject>().Where(x => string.Equals(
            (string?)x.GetValue("DesktopImage", StringComparison.OrdinalIgnoreCase), desktopImage, StringComparison.OrdinalIgnoreCase))
            .Take(2).ToArray();
        if (matches.Length != 1) return false;
        var property = matches[0].Property("Enabled", StringComparison.OrdinalIgnoreCase);
        if (property == null) matches[0]["Enabled"] = false;
        else property.Value = false;
        return true;
    }
}
