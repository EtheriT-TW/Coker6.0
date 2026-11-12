using EtheriT.Coker.Application.Shared.Dto.enumType.WebMenu;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Startup;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public sealed class DiagnosticsPlanBuilder(NavigationProvider navigation, CokerDbContext db,
    IActionDescriptorCollectionProvider actionDescriptors)
{
    public async Task<BackofficeTestListModel> BackofficeAsync()
    {
        var scope = await navigation.BuildDiagnosticsScopeAsync();
        return BackofficeTestCatalog.Build(actionDescriptors, scope.Jobs, true);
    }

    private static HashSet<string> PublicFeatures(IEnumerable<JobMenu> jobs)
    {
        var features = new HashSet<string>(StringComparer.Ordinal);
        foreach (var job in jobs.Where(job => job.Enable))
        {
            features.Add(job.PageName);
            if (job.jobItemModels != null) features.UnionWith(PublicFeatures(job.jobItemModels));
        }
        return features;
    }

    public async Task<PublicPlan> PublicAsync(long websiteId, CancellationToken token = default, bool functionScan = false)
    {
        using var guard = new DiagnosticsReadOnlySession(token);
        var scope = await navigation.BuildDiagnosticsScopeAsync();
        var features = PublicFeatures(scope.Jobs);
        var site = await db.Websites.AsNoTracking().SingleAsync(site => site.Id == websiteId, token);
        var targets = new List<PublicTarget>();
        var sourceLimited = false;
        if (!Uri.TryCreate(site.DefaultUrl?.TrimEnd('/') + "/", UriKind.Absolute, out var root) || !PublicProbeClient.IsHttp(root)
            || root.Query.Length != 0 || root.Fragment.Length != 0)
            return new(websiteId, "", targets, false, "網站的主要網址尚未設定為可檢測的公開 HTTP／HTTPS 網址。請先確認網站資料。");
        targets.Add(new("首頁", root.AbsoluteUri));
        var allowedLinks = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { root.GetLeftPart(UriPartial.Path).TrimEnd('/') };
        if (features.Contains("CustomPage")) allowedLinks.Add("site-prefix:" + new Uri(root, Uri.EscapeDataString(site.OrgName) + "/").AbsoluteUri);
        if (features.Contains("CustomPage") || features.Contains("MemberList") || features.Contains("OrderList"))
        {
            var menus = await db.WebMenus.AsNoTracking().Where(menu => menu.FK_WebsiteId == websiteId && menu.Visible && !menu.RemovedFromShelves)
                .OrderBy(menu => menu.SerNO).ThenBy(menu => menu.Id).Take(200).ToListAsync(token);
            sourceLimited = menus.Count == 200;
            var byId = menus.ToDictionary(menu => menu.Id);
            bool EnabledParents(EtheriT.Coker.Core.Models.WebMenu menu)
            {
                var visited = new HashSet<long>();
                while (menu.FK_TopNodeId is > 0)
                {
                    if (!visited.Add(menu.Id) || !byId.TryGetValue(menu.FK_TopNodeId.Value, out var parent)) return false;
                    menu = parent;
                }
                return true;
            }
            foreach (var menu in menus.Where(EnabledParents))
            {
                var supported = menu.PageType is PageTypeEnum.一般頁面 or PageTypeEnum.首頁 or PageTypeEnum.搜尋
                    ? features.Contains("CustomPage")
                    : menu.PageType == PageTypeEnum.會員 ? features.Contains("MemberList")
                    : menu.PageType == PageTypeEnum.購物車 && features.Contains("OrderList");
                if (!supported) continue;
                if (string.IsNullOrWhiteSpace(menu.RouterName) || !System.Text.RegularExpressions.Regex.IsMatch(menu.RouterName, @"^[\p{L}\p{N}_-]+$")) continue;
                var uri = new Uri(root, Uri.EscapeDataString(site.OrgName) + "/" + Uri.EscapeDataString(menu.RouterName));
                allowedLinks.Add(uri.GetLeftPart(UriPartial.Path).TrimEnd('/'));
                if (menu.PageType == PageTypeEnum.一般頁面) targets.Add(new(menu.Title ?? "網站頁面", uri.AbsoluteUri));
            }
        }
        if (features.Contains("Article"))
        {
            allowedLinks.Add("detail-prefix:" + new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/article/").AbsoluteUri);
            var now = DateTime.Now;
            var ids = await db.Article.AsNoTracking().Where(row => row.FK_WebsiteId == websiteId && row.Visible && !row.RemovedFromShelves
                && (row.permanent || (row.StartTime < now && row.EndTime > now)))
                .OrderBy(row => row.Id).Select(row => new { row.Id, row.Title }).Take(3).ToListAsync(token);
            targets.AddRange(ids.Select(row => new PublicTarget("文章：" + row.Title, new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/article/" + row.Id).AbsoluteUri)));
        }
        if (features.Contains("ProductList"))
        {
            allowedLinks.Add("detail-prefix:" + new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/product/").AbsoluteUri);
            var now = DateTime.Now;
            var ids = await db.Prods.AsNoTracking().Where(row => row.FK_WebsiteId == websiteId && row.Visible && !row.RemovedFromShelves
                && (row.permanent || (row.StartTime <= now && row.EndTime > now)))
                .OrderBy(row => row.Id).Select(row => new { row.Id, row.Title }).Take(3).ToListAsync(token);
            targets.AddRange(ids.Select(row => new PublicTarget("商品：" + row.Title, new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/product/" + row.Id).AbsoluteUri)));
        }
        if (features.Contains("TechnicalCertificate"))
        {
            allowedLinks.Add("detail-prefix:" + new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/techcert/").AbsoluteUri);
            var now = DateTime.Now;
            var ids = await db.TechnicalCertificates.AsNoTracking().Where(row => row.FK_WebsiteId == websiteId && row.Disp_opt
                && (row.Permanent || ((!row.StartDate.HasValue || row.StartDate <= now) && (!row.EndDate.HasValue || row.EndDate >= now))))
                .OrderBy(row => row.Id).Select(row => new { row.Id, row.Title }).Take(3).ToListAsync(token);
            targets.AddRange(ids.Select(row => new PublicTarget("技術證照：" + row.Title, new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/techcert/" + row.Id).AbsoluteUri)));
        }
        targets = targets.DistinctBy(target => target.Url).ToList();
        allowedLinks.UnionWith(targets.Select(target => new Uri(target.Url).GetLeftPart(UriPartial.Path).TrimEnd('/')));
        var limited = sourceLimited || targets.Count > 20;
        targets = targets.Take(20).ToList();
        return new(websiteId, PublicPageChecks.Stamp(targets, features.Concat(allowedLinks)) + (functionScan ? "-functions-v3" : "-resources"), targets, limited, null)
        { AllowedLinks = allowedLinks, EnabledFeatures = features };
    }

}
