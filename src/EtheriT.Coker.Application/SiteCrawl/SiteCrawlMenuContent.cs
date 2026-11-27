using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.SiteCrawl;

public sealed class SiteCrawlMenuContent(CokerDbContext db, LoginUserData user, IWebsiteCacheStateAppService cache)
{
    internal async Task<SiteCrawlMenuWrite?> PrepareAsync(long websiteId, ImportedSiteMenu menu,
        SiteLayoutScanner.LayoutPage page, SiteLayoutResult result, CancellationToken token, string? sourceHash = null)
    {
        if (menu.SourceUrl == null || menu.IsAlias) return null;
        var source = new Uri(menu.SourceUrl);
        var key = result.ImportState?.MenuKey(source) ?? "Menu|" + SiteLayoutScanner.PageKey(source);
        var previous = result.ImportState?.Find(key);
        if (previous?.NewId != menu.Id) previous = null;
        var hash = sourceHash ?? SiteCrawlFingerprint.ComputeSource(page.Content, menu.SourceTitle);
        var target = await db.WebMenus.AsNoTracking()
            .Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted && x.Id == menu.Id)
            .Select(x => new { x.CreationTime, Content = new SiteCrawlContentSnapshot(x.Html, x.Css, x.SaveHtml, x.SaveCss) })
            .SingleOrDefaultAsync(token);
        if (target == null) return null;
        var current = target.Content;
        // 還原或重建後可能重用選單 ID。只有比對照晚建立的空白選單才補寫，避免把人工清空的內容填回去。
        if (previous != null && current.IsEmpty && SiteCrawlReimport.WasRecreated(target.CreationTime, previous.ImportedAt))
            previous = null;
        var decision = result.DecideImport(previous, hash, current, $"選單「{menu.Title}」", isMenu: true);
        return decision == SiteCrawlImportDecision.Unchanged ? null : new(key, source, hash, current, previous, decision);
    }

    internal async Task<bool> CanWriteAsync(long websiteId, ImportedSiteMenu menu, SiteCrawlMenuWrite write,
        string html, string css, SiteLayoutResult result, CancellationToken token, bool hideGlobalFooter = false)
    {
        var latest = await ReadAsync(websiteId, menu.Id, token);
        if (write.Decision == SiteCrawlImportDecision.Write
            && result.CheckContentUnchanged(write.Content, latest, $"選單「{menu.Title}」")) return true;
        if (latest != null)
            await result.StageConflictAsync(SiteCrawlConflictKind.Menu, menu.Id, write.Key, write.Source, write.Hash,
                menu.SourceTitle, latest, write.Previous, html, css, SiteMenuImporter.LimitMenuTitle(menu.SourceTitle), hideGlobalFooter);
        return false;
    }

    private Task<SiteCrawlContentSnapshot?> ReadAsync(long websiteId, long menuId, CancellationToken token) =>
        db.WebMenus.AsNoTracking().Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted && x.Id == menuId)
            .Select(x => new SiteCrawlContentSnapshot(x.Html, x.Css, x.SaveHtml, x.SaveCss)).SingleOrDefaultAsync(token);

    internal async Task CompleteAsync(long websiteId, ImportedSiteMenu menu, SiteCrawlMenuWrite write, SiteLayoutResult result)
    {
        var query = db.WebMenus.Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted && x.Id == menu.Id);
        var saved = await query.AsNoTracking().Select(x => new { x.Html, x.Css, x.Title, x.SaveHtml, x.SaveCss }).SingleOrDefaultAsync();
        if (saved == null) return;
        var writtenTitle = saved.Title;
        if (write.Previous is { } previous)
        {
            writtenTitle = previous.WrittenTitle;
            if (SiteCrawlReimport.ShouldUpdate(menu.SourceTitle, previous.SourceTitle, saved.Title, previous.WrittenTitle))
            {
                var title = SiteMenuImporter.LimitMenuTitle(menu.SourceTitle);
                if (title != saved.Title)
                {
                    var entity = await query.SingleAsync();
                    entity.Title = title;
                    await user.SaveChanges(entity);
                    await cache.TouchByWebsiteIdAsync(websiteId, WebsiteCacheKeys.Menu);
                }
                writtenTitle = title;
            }
        }
        if (!result.CreatedMenuIds.Contains(menu.Id)) result.MenusUpdated++;
        result.ImportState?.RecordWritten(write.Key, menu.Id, write.Source, menu.SourceTitle, write.Hash,
            new(SiteCrawlFingerprint.ComputeWritten(saved.Html, saved.Css), writtenTitle,
                DraftHash: SiteCrawlFingerprint.ComputeWritten(saved.SaveHtml, saved.SaveCss)));
    }
}

internal sealed record SiteCrawlMenuWrite(string Key, Uri Source, string Hash, SiteCrawlContentSnapshot Content,
    SiteCrawlStateItem? Previous, SiteCrawlImportDecision Decision);
