using System.Text.Json;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType.WebMenu;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Diagnostics;
using EtheriT.Coker.Web.MVC.Views.Shared.Components.Sidebar;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.MVC.Controllers;

public partial class SystemDiagnosticsController
{
    private PublicResultStore PublicStore => new(HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().ContentRootPath);

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

    private async Task<PublicPlan> BuildPublicPlan(long websiteId)
    {
        using var guard = new DiagnosticsReadOnlySession(HttpContext.RequestAborted);
        var scope = await navigation.BuildDiagnosticsScopeAsync();
        var features = PublicFeatures(scope.Jobs);
        var db = HttpContext.RequestServices.GetRequiredService<CokerDbContext>();
        var site = await db.Websites.AsNoTracking().SingleAsync(site => site.Id == websiteId, HttpContext.RequestAborted);
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
                .OrderBy(menu => menu.SerNO).ThenBy(menu => menu.Id).Take(200).ToListAsync(HttpContext.RequestAborted);
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
                .OrderBy(row => row.Id).Select(row => new { row.Id, row.Title }).Take(3).ToListAsync(HttpContext.RequestAborted);
            targets.AddRange(ids.Select(row => new PublicTarget("文章：" + row.Title, new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/article/" + row.Id).AbsoluteUri)));
        }
        if (features.Contains("ProductList"))
        {
            allowedLinks.Add("detail-prefix:" + new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/product/").AbsoluteUri);
            var now = DateTime.Now;
            var ids = await db.Prods.AsNoTracking().Where(row => row.FK_WebsiteId == websiteId && row.Visible && !row.RemovedFromShelves
                && (row.permanent || (row.StartTime <= now && row.EndTime > now)))
                .OrderBy(row => row.Id).Select(row => new { row.Id, row.Title }).Take(3).ToListAsync(HttpContext.RequestAborted);
            targets.AddRange(ids.Select(row => new PublicTarget("商品：" + row.Title, new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/product/" + row.Id).AbsoluteUri)));
        }
        if (features.Contains("TechnicalCertificate"))
        {
            allowedLinks.Add("detail-prefix:" + new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/techcert/").AbsoluteUri);
            var now = DateTime.Now;
            var ids = await db.TechnicalCertificates.AsNoTracking().Where(row => row.FK_WebsiteId == websiteId && row.Disp_opt
                && (row.Permanent || ((!row.StartDate.HasValue || row.StartDate <= now) && (!row.EndDate.HasValue || row.EndDate >= now))))
                .OrderBy(row => row.Id).Select(row => new { row.Id, row.Title }).Take(3).ToListAsync(HttpContext.RequestAborted);
            targets.AddRange(ids.Select(row => new PublicTarget("技術證照：" + row.Title, new Uri(root, Uri.EscapeDataString(site.OrgName) + "/search/techcert/" + row.Id).AbsoluteUri)));
        }
        targets = targets.DistinctBy(target => target.Url).ToList();
        allowedLinks.UnionWith(targets.Select(target => new Uri(target.Url).GetLeftPart(UriPartial.Path).TrimEnd('/')));
        var limited = sourceLimited || targets.Count > 20;
        targets = targets.Take(20).ToList();
        return new(websiteId, PublicPageChecks.Stamp(targets, features.Concat(allowedLinks)), targets, limited, null) { AllowedLinks = allowedLinks };
    }

    private async Task<IActionResult> PublicPage(bool showAll)
    {
        if (!await loginUserData.IsPlatformAdministrator()) return Forbid();
        Response.Headers.CacheControl = "no-store";
        var site = await loginUserData.GetWebsiteId();
        var user = await loginUserData.GetUserId();
        HttpContext.RequestServices.GetRequiredService<BackgroundOperationContext>().Set(site, user);
        var plan = await BuildPublicPlan(site);
        PublicReport? report = null;
        string? warning = null;
        try { report = await PublicStore.LoadAsync(site, user, plan.Stamp); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        { logger.LogWarning(error, "Could not load public diagnostics for site {Site}", site); warning = "無法讀取上次前台紀錄，仍可重新檢測。"; }
        ViewData["Title"] = "PublicFunctionTests";
        return View("Public", new PublicDiagnosticsModel(plan, report, showAll, warning));
    }

    [HttpPost("/api/SystemDiagnostics/RunPublic"), ValidateAntiForgeryToken]
    public async Task<IActionResult> RunPublic([FromForm] long websiteId, [FromForm] int pageIndex, [FromForm] string scopeStamp)
    {
        Response.Headers.CacheControl = "no-store";
        if (!await loginUserData.IsPlatformAdministrator()) return Forbid();
        if (websiteId <= 0 || websiteId != await loginUserData.GetWebsiteId()) return StatusCode(409);
        var user = await loginUserData.GetUserId();
        HttpContext.RequestServices.GetRequiredService<BackgroundOperationContext>().Set(websiteId, user);
        var plan = await BuildPublicPlan(websiteId);
        if (plan.Error != null || scopeStamp != plan.Stamp) return StatusCode(409);
        if (pageIndex < 0 || pageIndex >= plan.Targets.Count) return BadRequest();
        using var gate = DiagnosticsRunGate.TryEnter(websiteId, user);
        if (gate == null) return StatusCode(429);
        var result = await PublicBrowserChecks.RunAsync(plan.Targets[pageIndex], plan.AllowedLinks,
            HttpContext.RequestServices.GetRequiredService<IConfiguration>(), logger, HttpContext.RequestAborted);
        string? warning = null;
        try { await PublicStore.SaveAsync(websiteId, user, plan.Stamp, result); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
        { logger.LogWarning(error, "Could not save public diagnostics for site {Site}", websiteId); warning = "本次前台結果未能儲存，請聯絡維護人員。"; }
        return PartialView("_PublicResults", new PublicDiagnosticsModel(plan,
            new PublicReport(plan.Stamp, DateTime.UtcNow, new() { [result.Url] = result }), true, warning));
    }
}
