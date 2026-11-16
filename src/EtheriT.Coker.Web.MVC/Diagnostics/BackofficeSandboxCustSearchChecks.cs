using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task CustSearchSettings(BackofficeSandbox box)
    {
        const long site = 999000001;
        const long foreign = 999000002;
        var prodTag = new Tag { FK_WebsiteId = site, Title = "商品範圍" };
        var articleTag = new Tag { FK_WebsiteId = site, Title = "文章範圍" };
        var foreignTag = new Tag { FK_WebsiteId = foreign, Title = "其他站" };
        var menu = new WebMenu { FK_WebsiteId = site, Title = "選單範圍", RouterName = "search-fixture", Visible = true };
        var foreignMenu = new WebMenu { FK_WebsiteId = foreign, Title = "其他站選單", RouterName = "foreign-fixture" };
        var foreignSearch = new CustSearch { FK_WebsiteId = foreign, Title = "其他站搜尋", Placeholder = "", SearchAllArticle = true, Visible = true, IsDefault = true };
        box.Db.Tags.AddRange(prodTag, articleTag, foreignTag);
        box.Db.WebMenus.AddRange(menu, foreignMenu);
        box.Db.CustSearch.Add(foreignSearch);
        await box.Db.SaveChangesAsync();
        var directoryCount = await box.Db.Directory.CountAsync();
        var service = box.Services.GetRequiredService<ICustSearchAppService>();
        var builtin = await service.GetDataOne(SearchTargetIds.Article);
        Require(builtin != null && builtin.IsSystem && builtin.SearchAllArticle && builtin.IsDefault,
            "A new site without products must expose the article fallback without inserting settings during reads.");
        Require(!await box.Db.CustSearch.AnyAsync(x => x.FK_WebsiteId == site), "Reading defaults must not write rows.");
        Require(await service.GetDataOne(SearchTargetIds.Product) == null && !(await service.SetDefault(SearchTargetIds.Product)).Success,
            "A site without visible products must neither expose nor accept the product default.");
        Require(!(await service.AddUp(new() { Title = "無商品範圍", SearchAllProd = true })).Success,
            "Product scope cannot be added when no searchable products exist.");
        (await box.Db.Websites.SingleAsync(x => x.Id == site)).Level = (EtheriT.Coker.Application.Shared.Dto.enumType.WebsiteLevelEnum)1;
        await box.Db.SaveChangesAsync();
        foreach (var kind in new[] { "hidden", "removed", "expired", "future", "deleted", "foreign" })
        {
            var fixture = await CreateProduct(box);
            var fixtureRow = await box.Db.Prods.SingleAsync(x => x.Id == fixture.Id);
            if (kind == "hidden") fixtureRow.Visible = false;
            if (kind == "removed") fixtureRow.RemovedFromShelves = true;
            if (kind == "deleted") fixtureRow.IsDeleted = true;
            if (kind == "foreign") fixtureRow.FK_WebsiteId = foreign;
            if (kind == "expired" || kind == "future")
            {
                fixtureRow.permanent = false;
                fixtureRow.StartTime = DateTime.Now.AddDays(kind == "expired" ? -3 : 1);
                fixtureRow.EndTime = DateTime.Now.AddDays(kind == "expired" ? -1 : 3);
            }
            await box.Db.SaveChangesAsync();
        }
        Require(!(await service.GetOptions()).HasProducts && await service.GetDataOne(-2) == null &&
            !(await service.GetSearchList(site)).Any(x => x.Id == -2),
            "Hidden, removed, expired, future, deleted and foreign fixtures must not enable product search.");
        var visibleProduct = await CreateProduct(box);
        Require((await service.GetOptions()).HasProducts && await service.GetDataOne(-2) != null &&
            (await service.GetSearchList(site)).Any(x => x.Id == -2), "A visible in-period product must enable product search in both menus.");
        directoryCount = await box.Db.Directory.CountAsync();
        var options = await service.GetOptions();
        Require(options.Tags.Count == 0 && options.Menus.All(x => x.Id != foreignMenu.Id), "Scope options must remain site-scoped.");
        var input = new CustSearchEditDto
        {
            Title = "混合搜尋", Description = "三類內容", Placeholder = "請輸入關鍵字", Visible = true, IsDefault = true, SerNO = 8,
            ProductTagIds = new() { prodTag.Id, prodTag.Id }, ArticleTagIds = new() { articleTag.Id }, MenuIds = new() { menu.Id }
        };
        Success(await service.AddUp(input));
        box.Db.ChangeTracker.Clear();
        var row = await box.Db.CustSearch.SingleAsync(x => x.FK_WebsiteId == site && x.SystemTargetId == null);
        var id = row.Id;
        var ordered = await service.GetAll(new DevExtreme.AspNet.Mvc.DataSourceLoadOptions { Skip = 0, Take = 3, RequireTotalCount = true });
        var orderedIds = Newtonsoft.Json.Linq.JObject.FromObject(ordered.Value!)["data"]!.Select(x => (long)x["Id"]!).ToArray();
        Require(orderedIds.SequenceEqual(new[] { SearchTargetIds.Product, SearchTargetIds.Article, id }),
            "Default list ordering must keep fixed entries first without depending on grid sort metadata.");
        var pageTwo = await service.GetAll(new DevExtreme.AspNet.Mvc.DataSourceLoadOptions { Skip = 2, Take = 1 });
        Require((long)Newtonsoft.Json.Linq.JObject.FromObject(pageTwo.Value!)["data"]![0]!["Id"]! == id,
            "Default list ordering must stay stable across paging.");
        var saved = await service.GetDataOne(id);
        Require(saved != null && saved.Title == input.Title && saved.Description == input.Description && saved.Placeholder == input.Placeholder &&
            saved.SerNO == 8 && !saved.IsDefault && saved.ProductTagIds.SequenceEqual(new[] { prodTag.Id }) &&
            saved.ArticleTagIds.SequenceEqual(new[] { articleTag.Id }) && saved.MenuIds.SequenceEqual(new[] { menu.Id }),
            "Create and read must retain separate mixed scopes and display fields without setting a default through AddUp.");
        Require(saved!.SelectedTags.Select(x => x.Id).ToHashSet().SetEquals(new[] { prodTag.Id, articleTag.Id }),
            "Detail reads must load only the selected same-site tag titles for the shared tag picker.");
        Success(await service.SetDefault(id));
        saved = await service.GetDataOne(id);
        saved!.Title = "更新搜尋"; saved.SearchAllProd = true; saved.IsDefault = false;
        Success(await service.AddUp(saved));
        var updated = await service.GetDataOne(id);
        Require(updated != null && updated.IsDefault && updated.Title == "更新搜尋" && updated.SearchAllProd && updated.ProductTagIds.Count == 0 &&
            updated.ArticleTagIds.SequenceEqual(new[] { articleTag.Id }), "All-products must clear only its redundant selected product tags.");
        updated!.Visible = false;
        Require(!(await service.AddUp(updated)).Success, "The current default cannot be hidden before choosing another default.");
        var system = await service.GetDataOne(SearchTargetIds.Product);
        system!.Title = "本站商品"; system.IsDefault = true; system.SearchAllArticle = true;
        Require(!(await service.AddUp(system)).Success, "Fixed item editing must be rejected by the service, not only hidden in the UI.");
        Success(await service.SetDefault(SearchTargetIds.Product));
        box.Db.ChangeTracker.Clear();
        Require(await box.Db.CustSearch.CountAsync(x => x.FK_WebsiteId == site && x.IsDefault && !x.IsDeleted) == 1 &&
            (await service.GetDataOne(id))!.IsDefault == false, "Changing default must clear only the same site's previous default.");
        var preserved = await service.GetDataOne(id);
        Require(preserved!.Title == "更新搜尋" && preserved.SearchAllProd && preserved.ArticleTagIds.SequenceEqual(new[] { articleTag.Id }) &&
            preserved.MenuIds.SequenceEqual(new[] { menu.Id }), "Selecting a list default must not rewrite other search fields or scopes.");
        var product = await service.GetDataOne(SearchTargetIds.Product);
        Require(product!.Id == -2 && product.IsSystem && product.SearchAllProd && !product.SearchAllArticle && product.Title == "商品",
            "Fixed items must retain their original name, target and scope after a default-only change.");
        Require(!(await service.Delete(-2)).Success, "Built-in searches must not be deletable.");
        Require(!(await service.AddUp(new() { Title = "", SearchAllArticle = true })).Success, "Blank title must be rejected.");
        Require(!(await service.AddUp(new() { Title = "無範圍" })).Success, "Empty search scope must be rejected.");
        Success(await service.AddUp(new() { Title = "隱藏搜尋", Visible = false, IsDefault = true, SearchAllArticle = true }));
        var hiddenId = await box.Db.CustSearch.Where(x => x.FK_WebsiteId == site && x.Title == "隱藏搜尋").Select(x => x.Id).SingleAsync();
        Require(!(await service.SetDefault(hiddenId)).Success, "A hidden custom item cannot become the list default.");
        Require(!(await service.AddUp(new() { Title = "越權標籤", ProductTagIds = new() { foreignTag.Id } })).Success, "Foreign tag must be rejected.");
        Require(!(await service.AddUp(new() { Title = "越權選單", MenuIds = new() { foreignMenu.Id } })).Success, "Foreign menu must be rejected.");
        Require(await service.GetDataOne(foreignSearch.Id) == null && !(await service.Delete(foreignSearch.Id)).Success &&
            !(await service.AddUp(new() { Id = foreignSearch.Id, Title = "不可修改", SearchAllArticle = true })).Success,
            "Foreign search read, update and deletion must be rejected.");
        Require(!(await service.AddUp(new() { Id = long.MaxValue, Title = "不存在", SearchAllArticle = true, IsDefault = true })).Success,
            "Missing targets must fail before changing the existing default.");
        Require(!(await service.SetDefault(foreignSearch.Id)).Success && !(await service.SetDefault(long.MaxValue)).Success,
            "Default selection must reject missing and foreign targets.");
        Require((await service.GetDataOne(SearchTargetIds.Product))!.IsDefault, "Failed default changes must preserve the previous selection.");
        Success(await service.AddUp(new() { Title = "純商品搜尋", SearchAllProd = true }));
        var productOnlyId = await box.Db.CustSearch.Where(x => x.FK_WebsiteId == site && x.Title == "純商品搜尋").Select(x => x.Id).SingleAsync();
        (await box.Db.Prods.SingleAsync(x => x.Id == visibleProduct.Id)).Visible = false;
        await box.Db.SaveChangesAsync();
        Require(!(await service.GetOptions()).HasProducts && await service.GetDataOne(-2) == null &&
            (await service.GetDataOne(-1))!.IsDefault, "The effective default must fall back to articles when the last visible product disappears.");
        Require(await box.Db.CustSearch.AnyAsync(x => x.FK_WebsiteId == site && x.SystemTargetId == -2 && x.IsDefault),
            "Availability reads must not erase the stored product default.");
        Require(!(await service.GetDataOne(productOnlyId))!.IsAvailable && !(await service.SetDefault(productOnlyId)).Success,
            "A retained product-only custom search without products cannot become the effective default.");
        var retained = await service.GetDataOne(id);
        retained!.Description = "商品暫時不可用時保留原範圍";
        Success(await service.AddUp(retained));
        Require((await service.GetDataOne(id))!.SearchAllProd, "Editing another field must not silently drop a retained product scope.");
        retained.SearchAllProd = false;
        Success(await service.AddUp(retained));
        Require(!(await service.GetDataOne(id))!.SearchAllProd, "An explicit removal of retained product scope must remain possible.");
        Success(await service.SetDefault(id));
        Success(await service.Delete(id));
        box.Db.ChangeTracker.Clear();
        Require(await service.GetDataOne(id) == null && (await box.Db.CustSearch.IgnoreQueryFilters().SingleAsync(x => x.Id == id)).IsDeleted,
            "Delete must soft-delete the custom setting and hide it from subsequent reads.");
        Require((await box.Db.CustSearch.SingleAsync(x => x.Id == foreignSearch.Id)).IsDefault && await box.Db.Directory.CountAsync() == directoryCount,
            "Search editing must not change foreign defaults or Directory rows.");
        Require((await service.GetDataOne(SearchTargetIds.Article))!.IsDefault, "Deleting the custom default must restore the site's automatic fallback.");
        (await box.Db.Prods.SingleAsync(x => x.Id == visibleProduct.Id)).Visible = true;
        await box.Db.SaveChangesAsync();
        Require((await service.GetOptions()).HasProducts && await service.GetDataOne(-2) != null,
            "Restoring a visible product must restore the product option without rebuilding settings.");
        Require(!await box.Db.SearchLogs.AnyAsync(), "Admin settings must not create keyword-search logs.");
    }
}
