using System.Globalization;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.SiteCrawl;

public sealed class SiteCrawlConflictService(CokerDbContext db, LoginUserData user, BackgroundTaskService tasks,
    SiteCrawlPendingStore pending, SiteCrawlStateStore states, IWebMenuApplication menus, IArticleAppService articles,
    IProductAppService products, ITemplatesApplicationService templates, IWebsiteCacheStateAppService cache,
    ILogger<SiteCrawlConflictService> logger)
{
    public async Task<IReadOnlyList<SiteCrawlPendingSummary>> ListAsync(long taskId, CancellationToken token)
    {
        var site = await user.GetWebsiteId();
        var org = await user.GetWebsiteOrgName(site);
        var items = await pending.ReadAsync(org, taskId, token);
        if (items.Count == 0) return [];
        var publicUrl = await db.Websites.AsNoTracking().Where(x => x.Id == site && !x.IsDeleted)
            .Select(x => x.DefaultUrl).SingleOrDefaultAsync(token);
        var menuIds = items.Where(x => x.Kind == SiteCrawlConflictKind.Menu).Select(x => x.NewId).Distinct().ToArray();
        var menuRoutes = await db.WebMenus.AsNoTracking()
            .Where(x => x.FK_WebsiteId == site && !x.IsDeleted && menuIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.RouterName, token);
        return items.Select(x => new SiteCrawlPendingSummary(x.Id, x.Kind.ToString(), x.Title, x.SourceUrl,
            NewSiteUrl(publicUrl, org, x, menuRoutes.GetValueOrDefault(x.NewId)))).ToList();
    }

    private static string? NewSiteUrl(string? publicUrl, string org, SiteCrawlPendingItem item, string? routerName)
    {
        if (string.IsNullOrWhiteSpace(org) || !Uri.TryCreate(publicUrl?.Trim().TrimEnd('/') + "/", UriKind.Absolute, out var root)
            || root.Scheme is not ("http" or "https") || root.UserInfo.Length != 0
            || root.Query.Length != 0 || root.Fragment.Length != 0) return null;
        var id = item.NewId.ToString(CultureInfo.InvariantCulture);
        var route = item.Kind switch
        {
            SiteCrawlConflictKind.Menu when !string.IsNullOrWhiteSpace(routerName) && routerName is not ("." or "..")
                && !routerName.Contains('/') && !routerName.Contains('\\') => Uri.EscapeDataString(routerName),
            SiteCrawlConflictKind.Article => $"search/article/{id}",
            SiteCrawlConflictKind.Product => $"search/product/{id}",
            SiteCrawlConflictKind.Footer => "Home",
            _ => null
        };
        if (route == null) return null;

        // 前台內容路由需要組織名稱。主要網址已帶該段時直接接內容路徑，避免重複。
        var lastSegment = root.AbsolutePath.TrimEnd('/').Split('/')[^1];
        var prefix = string.Equals(Uri.UnescapeDataString(lastSegment), org, StringComparison.OrdinalIgnoreCase)
            ? "" : Uri.EscapeDataString(org) + "/";
        return new Uri(root, "./" + prefix + route).AbsoluteUri;
    }

    public async Task<IReadOnlyList<SiteCrawlConflictOutcome>> ApplyAsync(long taskId, IReadOnlySet<string> overwrite,
        CancellationToken token)
    {
        var websiteId = await user.GetWebsiteId();
        var org = await user.GetWebsiteOrgName(websiteId);
        using var connection = JobStorage.Current.GetConnection();
        using var operationLock = AcquireLock(connection, websiteId);
        if (await tasks.HasActiveSiteCrawlAsync(websiteId, token))
            throw new BackgroundTaskConflictException("網站匯入任務處理中，請等任務完成後再套用。");

        var items = await pending.ReadAsync(org, taskId, token);
        if (items.Count == 0 || !overwrite.IsSubsetOf(items.Select(x => x.Id)))
            throw new BackgroundTaskConflictException("這批衝突已變更，請重新載入匯入結果。");
        var state = await states.LoadAsync(org, new Uri(items[0].SourceUrl), token);
        var outcomes = new List<SiteCrawlConflictOutcome>();
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            if (!overwrite.Contains(item.Id))
            {
                pending.Remove(org, item);
                outcomes.Add(new(item.Id, item.Title, "kept"));
                continue;
            }
            if (state.Find(item.StateKey) is not { } baseline || baseline.NewId != item.NewId || baseline.SourceHash != item.SourceHash)
            {
                outcomes.Add(new(item.Id, item.Title, "changed"));
                continue;
            }

            try
            {
                var current = await ContentAsync(websiteId, item, token);
                if (current == null || !item.Matches(current.Html, current.Css, current.SaveHtml, current.SaveCss))
                {
                    outcomes.Add(new(item.Id, item.Title, "changed"));
                    continue;
                }
                if (!(await WriteAsync(item)).Success)
                {
                    outcomes.Add(new(item.Id, item.Title, "failed"));
                    continue;
                }
                var saved = await ContentAsync(websiteId, item, CancellationToken.None)
                    ?? throw new InvalidOperationException("套用後找不到匯入資料。");
                var title = await RenameAsync(websiteId, item, baseline.WrittenTitle);
                if (item.Kind is SiteCrawlConflictKind.Article or SiteCrawlConflictKind.Product)
                    await cache.TouchByWebsiteIdAsync(websiteId, WebsiteCacheKeys.DirectoryContent);
                state.RecordWritten(item.StateKey, item.NewId, new Uri(item.SourceUrl), item.Title, item.SourceHash,
                    new(SiteCrawlFingerprint.ComputeWritten(saved.Html, saved.Css), title, baseline.WrittenFields,
                        SiteCrawlFingerprint.ComputeWritten(saved.SaveHtml, saved.SaveCss)), baseline.SourceFields);
                await state.SaveAsync();
                pending.Remove(org, item);
                outcomes.Add(new(item.Id, item.Title, "applied"));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Applying site crawl conflict {ItemId} for site {WebsiteId} failed", item.Id, websiteId);
                outcomes.Add(new(item.Id, item.Title, "failed"));
            }
        }
        return outcomes;
    }

    internal static IDisposable AcquireLock(IStorageConnection connection, long websiteId)
    {
        try
        {
            return connection.AcquireDistributedLock(BackgroundTaskService.SiteCrawlKey(websiteId), TimeSpan.FromSeconds(1));
        }
        catch (DistributedLockTimeoutException)
        {
            throw new BackgroundTaskConflictException("網站匯入任務或另一個套用操作處理中，請稍後再試。");
        }
    }

    private Task<SiteCrawlContentSnapshot?> ContentAsync(long site, SiteCrawlPendingItem item, CancellationToken token) =>
        item.Kind switch
        {
            SiteCrawlConflictKind.Menu => db.WebMenus.AsNoTracking().Where(x => x.Id == item.NewId && x.FK_WebsiteId == site && !x.IsDeleted)
                .Select(x => new SiteCrawlContentSnapshot(x.Html, x.Css, x.SaveHtml, x.SaveCss)).SingleOrDefaultAsync(token),
            SiteCrawlConflictKind.Article => db.Article.AsNoTracking().Where(x => x.Id == item.NewId && x.FK_WebsiteId == site && !x.IsDeleted)
                .Select(x => new SiteCrawlContentSnapshot(x.Html, x.Css, x.SaveHtml, x.SaveCss)).SingleOrDefaultAsync(token),
            SiteCrawlConflictKind.Product => db.Prods.AsNoTracking().Where(x => x.Id == item.NewId && x.FK_WebsiteId == site && !x.IsDeleted)
                .Select(x => new SiteCrawlContentSnapshot(x.Html, x.Css, x.SaveHtml, x.SaveCss)).SingleOrDefaultAsync(token),
            SiteCrawlConflictKind.Footer => db.FooterTemplates.AsNoTracking()
                .Where(x => x.Id == item.NewId && !x.IsDeleted && !x.templateSections.IsDeleted
                    && !x.templateSections.template.IsDeleted && x.templateSections.template.FK_WebsiteID == site)
                .Select(x => new SiteCrawlContentSnapshot(x.html, x.css, x.saveHtml, x.saveCss)).SingleOrDefaultAsync(token),
            _ => throw new InvalidOperationException("無效的衝突類型。")
        };

    private Task<ResponseMessageDto> WriteAsync(SiteCrawlPendingItem item)
    {
        var id = checked((int)item.NewId);
        return item.Kind switch
        {
            SiteCrawlConflictKind.Menu => menus.importConten(new MenuSaveContenDto { Id = id, SaveHtml = item.Html, SaveCss = item.Css }),
            SiteCrawlConflictKind.Article => articles.ImportConten(new ArticleSaveContenDto { Id = id, SaveHtml = item.Html, SaveCss = item.Css }),
            SiteCrawlConflictKind.Product => products.ImportConten(new ProdSaveContenDto { Id = id, SaveHtml = item.Html, SaveCss = item.Css }),
            SiteCrawlConflictKind.Footer => templates.importDefaultFooter(new MenuSaveContenDto { Id = id, SaveHtml = item.Html, SaveCss = item.Css }),
            _ => throw new InvalidOperationException("無效的衝突類型。")
        };
    }

    private async Task<string?> RenameAsync(long site, SiteCrawlPendingItem item, string? previous)
    {
        if ((item.RenameTo == null || item.RenameFrom == null) && !item.HideGlobalFooter) return previous;
        switch (item.Kind)
        {
            case SiteCrawlConflictKind.Menu:
                var menu = await db.WebMenus.SingleOrDefaultAsync(x => x.Id == item.NewId && x.FK_WebsiteId == site
                    && !x.IsDeleted);
                if (menu == null) return previous;
                var rename = item.RenameTo != null && menu.Title == item.RenameFrom;
                if (!rename && (!item.HideGlobalFooter || !menu.VisibleFooter)) return previous;
                if (rename) menu.Title = item.RenameTo;
                if (item.HideGlobalFooter) menu.VisibleFooter = false;
                await user.SaveChanges(menu);
                await cache.TouchByWebsiteIdAsync(site, WebsiteCacheKeys.Menu);
                return rename ? item.RenameTo : previous;
            case SiteCrawlConflictKind.Article:
                var article = await db.Article.SingleOrDefaultAsync(x => x.Id == item.NewId && x.FK_WebsiteId == site
                    && !x.IsDeleted && x.Title == item.RenameFrom);
                if (article == null || article.Title != item.RenameFrom) return previous;
                article.Title = item.RenameTo;
                await user.SaveChanges(article);
                break;
            case SiteCrawlConflictKind.Product:
                var product = await db.Prods.SingleOrDefaultAsync(x => x.Id == item.NewId && x.FK_WebsiteId == site
                    && !x.IsDeleted && x.Title == item.RenameFrom);
                if (product == null || product.Title != item.RenameFrom) return previous;
                product.Title = item.RenameTo;
                await user.SaveChanges(product);
                break;
            default: return previous;
        }
        return item.RenameTo;
    }
}

public sealed record SiteCrawlPendingSummary(string Id, string Kind, string Title, string SourceUrl, string? NewUrl);
public sealed record SiteCrawlConflictOutcome(string Id, string Title, string Status);
