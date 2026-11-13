using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.Article;
using EtheriT.Coker.Application.Shared.Dto.Directory;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.Dto.UserHabits;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application.Shared.UserHabits;
using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task Crud<TEntity, TInput>(BackofficeSandbox box, TInput input,
        Func<TInput, Task<ResponseMessageDto>> save, Func<long, Task<ResponseMessageDto>> delete,
        Action<TInput, long> mutate, Func<TEntity, string?> name) where TEntity : FullAuditedEntity
    {
        var before = (await box.Db.Set<TEntity>().IgnoreQueryFilters().Select(row => row.Id).ToListAsync()).ToHashSet();
        Success(await save(input)); box.Db.ChangeTracker.Clear();
        var created = await box.Db.Set<TEntity>().IgnoreQueryFilters().AsNoTracking().Where(row => !before.Contains(row.Id)).ToListAsync();
        Require(created.Count == 1, "Create must persist exactly one fixture record.");
        var id = created[0].Id;
        mutate(input, id);
        Success(await save(input)); box.Db.ChangeTracker.Clear();
        var updated = await box.Db.Set<TEntity>().IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == id);
        Require(name(updated) == "更新檢測資料", "Update did not persist expected data.");
        Success(await delete(id)); box.Db.ChangeTracker.Clear();
        var removed = await box.Db.Set<TEntity>().IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(row => row.Id == id);
        Require(removed == null || removed.IsDeleted, "Delete did not remove or soft-delete the fixture.");
    }

    private static async Task CertificateCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<ITechnicalCertificateAppService>();
        await Crud<TechnicalCertificate, TechCertDto>(box,
            new() { Title = "檢測證照", Description = "檢測說明", Img = "", Permanent = true, Disp_opt = true },
            service.AddUp, service.Delete, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
        var foreign = new TechnicalCertificate { FK_WebsiteId = 999000002, Title = "其他站台證照" };
        box.Db.TechnicalCertificates.Add(foreign);
        await box.Db.SaveChangesAsync();
        var id = foreign.Id;
        box.Db.ChangeTracker.Clear();
        Require(!(await service.AddUp(new TechCertDto { Id = id, Title = "不應變更" })).Success, "Cross-site certificate edit was accepted.");
        Require(!(await service.Delete(id)).Success, "Cross-site certificate deletion was accepted.");
        box.Db.ChangeTracker.Clear();
        var unchanged = await box.Db.TechnicalCertificates.AsNoTracking().SingleAsync(row => row.Id == id);
        Require(unchanged.Title == "其他站台證照" && !unchanged.IsDeleted, "Cross-site certificate was modified.");
    }
    private static Task DirectoryCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IDirectoryAppService>();
        return Crud<EtheriT.Coker.Core.Models.Directory, DirectoryAddUpDto>(box,
            new() { Id = 0, Title = "檢測目錄", Description = "檢測說明", Type = 1, Visible = true },
            service.AddUp, service.Delete, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static async Task DirectoryAllContents(BackofficeSandbox box)
    {
        const long site = 999000001;
        const long foreignSite = 999000002;
        box.Services.GetRequiredService<IConfiguration>()["WebConfig:SiteId"] = site.ToString();
        var first = await CreateProduct(box);
        var second = await CreateProduct(box);
        (await box.Db.Prods.SingleAsync(p => p.Id == first.Id)).Title = "AlphaProduct";
        (await box.Db.Prods.SingleAsync(p => p.Id == second.Id)).Title = "BetaProduct";
        var hidden = await CreateProduct(box);
        var expired = await CreateProduct(box);
        var removed = await CreateProduct(box);
        var foreign = await CreateProduct(box);
        var restricted = await CreateProduct(box);
        box.Db.PermissionDetail.Add(new PermissionDetail { Name = "隔離會員限定", FK_WebsiteId = site, FK_TargetId = restricted.Id, FK_RoleId = 999000003, IsGranted = true, Type = (int)EtheriT.Coker.Application.Shared.Dto.enumType.PermissionDetailsTypeEnum.產品會員 });
        (await box.Db.Prods.SingleAsync(p => p.Id == hidden.Id)).Visible = false;
        var expiredRow = await box.Db.Prods.SingleAsync(p => p.Id == expired.Id);
        expiredRow.permanent = false; expiredRow.StartTime = DateTime.Now.AddDays(-2); expiredRow.EndTime = DateTime.Now.AddDays(-1);
        (await box.Db.Prods.SingleAsync(p => p.Id == removed.Id)).RemovedFromShelves = true;
        (await box.Db.Prods.SingleAsync(p => p.Id == foreign.Id)).FK_WebsiteId = foreignSite;
        var article1 = new Article { FK_WebsiteId = site, Title = "Alpha", Html = "<p>Alpha</p>", Visible = true, permanent = true };
        var article2 = new Article { FK_WebsiteId = site, Title = "Beta", Html = "<p>Beta</p>", Visible = true, permanent = true };
        box.Db.Article.AddRange(article1, article2,
            new Article { FK_WebsiteId = site, Title = "Hidden", Visible = false, permanent = true },
            new Article { FK_WebsiteId = foreignSite, Title = "Foreign", Visible = true, permanent = true },
            new Article { FK_WebsiteId = site, Title = "Expired", Visible = true, permanent = false, StartTime = DateTime.Now.AddDays(-2), EndTime = DateTime.Now.AddDays(-1) });
        box.Db.WebMenus.Add(new WebMenu { FK_WebsiteId = site, Title = "一般選單頁", Html = "<p>一般內容</p>", RouterName = "catalog-fixture", Visible = true });
        await box.Db.SaveChangesAsync(); box.Db.ChangeTracker.Clear();
        var service = box.Services.GetRequiredService<IDirectoryAppService>();
        DirectoryReleInfoInputDto Request(string source, int page = 1, int? maximum = null) => new()
        {
            ContentSource = source, SiteId = foreignSite, Ids = new() { 123456 },
            Page = page, ShowNum = 1, MaxLen = maximum, SearchSortBy = "name", SearchSortDirection = "asc"
        };
        var products = await service.GetReleInfo(Request("all-products"));
        var productPage2 = await service.GetReleInfo(Request("all-products", 2));
        Require(products.TotalCount == 2 && products.TotalPage == 2 && products.ReleInfos.Count == 1 && productPage2.ReleInfos.Count == 1,
            "Full-product listing must apply site, visibility, shelf dates and paging without keyword or directory tags.");
        Require(new[] { products.ReleInfos[0].Id, productPage2.ReleInfos[0].Id }.ToHashSet().SetEquals(new[] { first.Id, second.Id }),
            "Product pages must not repeat or leak hidden/foreign products.");
        var limited = await service.GetReleInfo(Request("all-products", 2, 1));
        Require(limited.TotalCount == 1 && limited.TotalPage == 1 && limited.ReleInfos.Count == 0, "MaxLen must limit both totals and final-page results.");
        var articles = await service.GetReleInfo(Request("all-articles"));
        var articlePage2 = await service.GetReleInfo(Request("all-articles", 2));
        Require(articles.TotalCount == 2 && articles.ReleInfos.Count == 1 && articlePage2.ReleInfos.Count == 1 &&
            new[] { articles.ReleInfos[0].Id, articlePage2.ReleInfos[0].Id }.ToHashSet().SetEquals(new[] { article1.Id, article2.Id }),
            "Full-article listing must include only visible in-period articles from the configured site.");
        Require(articles.ReleInfos.All(a => a.Link.StartsWith("/article/")) && products.ReleInfos.All(p => p.Link.StartsWith("/product/")),
            "Full-content details must retain product/article route kinds.");
        var productNeighbors = await service.SwitchPage(new DirectorySwitchPageDto { id = first.Id, routername = "search", type = 1 });
        Require(productNeighbors.Count == 2 && productNeighbors.Count(item => !string.IsNullOrEmpty(item.Key)) == 1 &&
            productNeighbors.Any(item => item.Key == second.Id.ToString()),
            "Canonical product navigation must return the other visible product and an empty boundary slot.");
        var reverseNeighbors = await service.SwitchPage(new DirectorySwitchPageDto { id = second.Id, routername = "search", type = 1 });
        Require(reverseNeighbors.Count == 2 && reverseNeighbors.Any(item => item.Key == first.Id.ToString()) &&
            reverseNeighbors.Count(item => !string.IsNullOrEmpty(item.Key)) == 1,
            "Both first and last canonical product pages must have correct boundaries.");
        Require((await service.SwitchPage(new DirectorySwitchPageDto { id = hidden.Id, routername = "search", type = 1 })).Count == 0,
            "Hidden canonical products must not receive neighbors from the public list.");
        var articleNeighbors = await service.SwitchPage(new DirectorySwitchPageDto { id = article1.Id, routername = "search", type = 2 });
        Require(articleNeighbors.Count == 2 && articleNeighbors.Any(item => item.Key == article2.Id.ToString()) &&
            articleNeighbors.Count(item => !string.IsNullOrEmpty(item.Key)) == 1,
            "Keyword-free article navigation must return article IDs without menu routes.");
        Require(!await box.Db.SearchLogs.AnyAsync(), "Full-content browsing must not write search logs.");
        var asc = await service.SwitchPage(new DirectorySwitchPageDto { id = first.Id, routername = "search", type = 1, SearchSortBy = "name", SearchSortDirection = "asc" });
        var desc = await service.SwitchPage(new DirectorySwitchPageDto { id = first.Id, routername = "search", type = 1, SearchSortBy = "name", SearchSortDirection = "desc" });
        Require(asc.Count == 2 && asc[1].Key == second.Id.ToString() && string.IsNullOrEmpty(asc[0].Key) &&
            desc.Count == 2 && desc[0].Key == second.Id.ToString() && string.IsNullOrEmpty(desc[1].Key),
            "Explicit sort direction must reverse canonical neighbors consistently with the catalog.");
        var filtered = await service.SwitchPage(new DirectorySwitchPageDto { id = first.Id, routername = "search", type = 1, ProductNameFilter = "AlphaProduct" });
        Require(filtered.Count == 2 && filtered.All(item => string.IsNullOrEmpty(item.Key)),
            "Product-name filtering must constrain both neighbor directions.");
        var limitedNeighbors = await service.SwitchPage(new DirectorySwitchPageDto { id = second.Id, routername = "search", type = 1, SearchSortBy = "name", SearchSortDirection = "asc", MaxLen = 1 });
        Require(limitedNeighbors.Count == 0, "Navigation must not escape the configured maximum result count.");
        var keywordFiltered = await service.SwitchPage(new DirectorySwitchPageDto { id = second.Id, routername = "search", type = 1,
            searchtext = "Product", ProductNameFilter = "BetaProduct", SearchSortBy = "name", SearchSortDirection = "desc" });
        Require(keywordFiltered.Count == 1 && keywordFiltered[0].Key == second.Id.ToString(),
            "Keyword-search navigation must use the same explicit sorting and product-name filter.");
        var keywordArticles = await service.SwitchPage(new DirectorySwitchPageDto { id = article1.Id, routername = "search", type = 2,
            searchtext = "Alpha", SearchSortBy = "title", SearchSortDirection = "asc" });
        Require(keywordArticles.Count == 1 && keywordArticles[0].Key == "/article/" + article1.Id,
            "Article keyword navigation must use the article target rather than automatically switching to products.");
        var invalidRejected = false;
        try { await service.GetReleInfo(Request("unsupported")); }
        catch (ArgumentException) { invalidRejected = true; }
        Require(invalidRejected, "Unknown content sources must not broaden the query.");
    }

    private static async Task MenuPublishedContentCache(BackofficeSandbox box)
    {
        const long site = 999000001;
        const long otherSite = 999000002;
        var menu = new WebMenu
        {
            FK_WebsiteId = site, Title = "內容快取檢測", RouterName = "content-cache-fixture", icon = "empty",
            PageType = EtheriT.Coker.Application.Shared.Dto.enumType.WebMenu.PageTypeEnum.一般頁面,
            Visible = true, ShowToMenu = true, Html = "", PageText = ""
        };
        box.Db.WebMenus.Add(menu);
        await box.Db.SaveChangesAsync();
        var service = box.Services.GetRequiredService<EtheriT.Coker.Application.IWebMenuApplication>();
        var versions = box.Services.GetRequiredService<EtheriT.Coker.Application.Shared.JsonObject.IWebsiteCacheStateAppService>();
        var key = EtheriT.Coker.Application.Shared.Dto.enumType.WebsiteCacheKeys.Menu;
        var initial = await service.GetDisplayAll(site);
        Require(initial.Success && !initial.Maps.Single(row => row.Id == menu.Id).hasContan,
            "Empty menu must be cached as non-navigable before publishing.");
        var before = await versions.GetVersionByWebsiteIdAsync(site, key);
        var otherBefore = await versions.GetVersionByWebsiteIdAsync(otherSite, key);
        Success(await service.saveConten(new MenuSaveContenDto { Id = checked((int)menu.Id), SaveHtml = "<p>僅草稿</p>", SaveCss = "" }));
        Require(await versions.GetVersionByWebsiteIdAsync(site, key) == before,
            "Draft-only saves must not invalidate published menu navigation.");
        Success(await service.importConten(new MenuSaveContenDto { Id = checked((int)menu.Id), SaveHtml = "<p>正式頁面內容</p>", SaveCss = "" }));
        var publishedVersion = await versions.GetVersionByWebsiteIdAsync(site, key);
        Require(publishedVersion > before, "Publishing canvas HTML must advance the menu version.");
        var published = await service.GetDisplayAll(site);
        Require(published.Success && published.Maps.Single(row => row.Id == menu.Id).hasContan,
            "Warm menu JSON must become navigable without editing menu metadata.");
        Success(await service.importConten(new MenuSaveContenDto { Id = checked((int)menu.Id), SaveHtml = "<p>更新正式文字</p>", SaveCss = "p { color: red; }" }));
        Require(await versions.GetVersionByWebsiteIdAsync(site, key) == publishedVersion,
            "Editing existing content or CSS must reuse the menu version.");
        Success(await service.importConten(new MenuSaveContenDto { Id = checked((int)menu.Id), SaveHtml = "<body><style>p { color: red; }</style><p>&nbsp;<br></p></body>", SaveCss = "p { color: red; }" }));
        box.Db.ChangeTracker.Clear();
        var emptyMenu = await box.Db.WebMenus.SingleAsync(row => row.Id == menu.Id);
        Require(emptyMenu.Html == null && emptyMenu.PageText == null && emptyMenu.SaveHtml == null && emptyMenu.Css == null && emptyMenu.SaveCss == null,
            "Empty canvas wrappers, whitespace entities and styles must clear published and draft markup to null.");
        var cleared = await service.GetDisplayAll(site);
        Require(cleared.Success && !cleared.Maps.Single(row => row.Id == menu.Id).hasContan &&
            await versions.GetVersionByWebsiteIdAsync(site, key) > publishedVersion,
            "Clearing published HTML must refresh the menu back to non-navigable.");
        await service.GetFrontConten(new() { siteId = site, key = menu.RouterName });
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.WebMenus.SingleAsync(row => row.Id == menu.Id)).Html == null,
            "Opening an intentionally cleared page must not restore an empty HTML shell or old content.");
        var clearedVersion = await versions.GetVersionByWebsiteIdAsync(site, key);
        Success(await service.importConten(new MenuSaveContenDto { Id = checked((int)menu.Id), SaveHtml = null, SaveCss = null }));
        Require(await versions.GetVersionByWebsiteIdAsync(site, key) == clearedVersion,
            "Publishing an already-empty canvas must not change the menu version.");
        Success(await service.importConten(new MenuSaveContenDto { Id = checked((int)menu.Id), SaveHtml = "<img src='/images/noImg.jpg' alt=''>", SaveCss = "" }));
        var imageVersion = await versions.GetVersionByWebsiteIdAsync(site, key);
        var imageMenu = await service.GetDisplayAll(site);
        Require(imageVersion > clearedVersion && imageMenu.Success && imageMenu.Maps.Single(row => row.Id == menu.Id).hasContan,
            "An image-only page is real content and must not be cleared as an empty canvas.");
        Success(await service.importConten(new MenuSaveContenDto { Id = checked((int)menu.Id), SaveHtml = "<p>再次清空前內容</p>", SaveCss = "" }));
        Require(await versions.GetVersionByWebsiteIdAsync(site, key) == imageVersion,
            "Switching image content to text must preserve the menu version.");
        Require(await versions.GetVersionByWebsiteIdAsync(otherSite, key) == otherBefore,
            "Content publication must not change another site's menu version.");
    }

    private static Task ArticleCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IArticleAppService>();
        return Crud<Article, ArticleDto>(box,
            new() { Title = "檢測文章", Description = "檢測說明", Visible = true, permanent = true, TagSelected = new() },
            service.AddUp, service.Delete, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static Task AdvertiseCrud(BackofficeSandbox box, int type)
    {
        var service = box.Services.GetRequiredService<IAdvertiseAppService>();
        return Crud<Advertise, AdvertiseDto>(box,
            new() { Title = "檢測廣告", Describe = "檢測說明", Type = type, Visible = true, permanent = true, TagSelected = new() },
            service.AddUp, service.Delete, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static async Task TagCrud(BackofficeSandbox box, bool group)
    {
        var service = box.Services.GetRequiredService<ITagAppService>();
        var input = new DevExpressDto { Values = JsonConvert.SerializeObject(new { Title = "檢測標籤", Disp_Opt = true, FK_Tid = new long[0] }) };
        void Mutate(DevExpressDto data, long id)
        { data.Key = checked((int)id); data.Values = JsonConvert.SerializeObject(new { Title = "更新檢測資料", Disp_Opt = true, FK_Tid = new long[0] }); }
        if (group) await Crud<Tag_Group, DevExpressDto>(box, input, service.TagGroupAddUp, service.TagGroupDelete, Mutate, row => row.Title);
        else await Crud<Tag, DevExpressDto>(box, input, service.TagAddUp, service.TagDelete, Mutate, row => row.Title);
    }
    private static Task UserGroupCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IUserHabitsAppService>();
        return Crud<UserGrouping, UserGroupAddUpInputDto>(box,
            new() { Title = "檢測分群", Description = "檢測說明", Enable = true }, service.AddUpUserGroup,
            service.DeleteUserGroup, (input, id) => { input.Id = id; input.Title = "更新檢測資料"; }, row => row.Title);
    }
    private static async Task MemberRoleCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IMemberAppService>();
        // The first role is a default role and must not be used as the deletion fixture.
        Success(await service.RoleAddUp(new DevExpressDto { Values = "{\"Name\":\"固定預設角色\"}" }));
        await Crud<Role, DevExpressDto>(box, new() { Values = "{\"Name\":\"檢測會員角色\"}" },
            service.RoleAddUp, service.RoleDelete, (input, id) => { input.Key = checked((int)id); input.Values = "{\"Name\":\"更新檢測資料\"}"; }, row => row.Name);
    }
    private static Task BoxCrud(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IFreightAppService>();
        return Crud<LogisticsBox, GetLogisticsBoxAllListInputDto>(box,
            new() { Name = "檢測箱型", CapacityPoint = 10, IsActive = true, Sort = 1 }, service.LogisticsBoxAddUp,
            service.LogisticsBoxDelete, (input, id) => { input.Id = id; input.Name = "更新檢測資料"; }, row => row.Name);
    }
}
