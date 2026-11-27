using EtheriT.Coker.Application.Common;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.SiteCrawl;

// 舊站跑馬燈匯入新系統「跑馬燈」（內容管理 > 跑馬燈，顯示位置＝頁首）；同標題已存在就不重複新增，重新匯入只補新的。
public sealed class SiteMarqueeImporter(CokerDbContext db, LoginUserData user)
{
    private const string HeaderPlacement = "Top";

    internal async Task ImportAsync(SiteLayoutSelection layout, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        // 跑馬燈在全站版頭，入口頁優先；入口頁沒抓到再找其他頁。
        var items = layout.Pages.Select(page => page.Marquee).FirstOrDefault(marquee => marquee is { Count: > 0 });
        if (items == null) return;
        result.SeenMarqueeKeys.UnionWith(items.Select(x => "Marquee|" + x.Title));

        await checkpoint();
        var websiteId = await user.GetWebsiteId();
        var existing = await db.Marquees.Where(x => x.FK_WebsiteId == websiteId && !x.IsDeleted)
            .Select(x => new { x.Id, x.title, x.ser_no }).ToListAsync(token);
        var ids = existing.Select(x => x.Id).ToHashSet();
        var byTitle = existing.Where(x => x.title != null).GroupBy(x => x.title!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Min(x => x.Id), StringComparer.Ordinal);
        var order = existing.Count == 0 ? 0 : existing.Max(x => x.ser_no);
        foreach (var item in items.DistinctBy(x => x.Title, StringComparer.Ordinal))
        {
            var key = "Marquee|" + item.Title;
            var previous = result.ImportState?.Find(key);
            var id = previous == null ? byTitle.GetValueOrDefault(item.Title) : ids.Contains(previous.NewId) ? previous.NewId : 0;
            var hash = SiteCrawlFingerprint.ComputeWritten(item.Title, item.Link?.AbsoluteUri);
            if (id > 0)
            {
                result.ClaimedRemovalTargets.Add(("Marquee", id));
                if (previous?.SourceHash == hash) result.CountUnchanged();
                else result.ImportState?.RecordWritten(key, id, item.Link, item.Title, hash, new(null, previous?.WrittenTitle ?? item.Title));
                continue;
            }
            var marquee = new Core.Models.Marquee
            {
                FK_WebsiteId = websiteId,
                placement = HeaderPlacement,
                title = item.Title,
                disp_opt = true,
                ser_no = ++order,
                link = item.Link == null ? "" : SiteCrawlHtml.FindRoute(result, item.Link) ?? item.Link.AbsoluteUri,
                target = false,
                permanent = true
            };
            db.Marquees.Add(marquee);
            await user.SaveChanges(marquee);
            await result.RecordImportTargetAsync(key, marquee.Id, item.Link, item.Title, created: true, writtenTitle: item.Title);
            result.ImportState?.RecordWritten(key, marquee.Id, item.Link, item.Title, hash, new(null, marquee.title));
            await result.SaveImportStateAsync();
            result.MarqueesImported++;
        }
    }
}
