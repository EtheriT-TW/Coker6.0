using System.Data;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EtheriT.Coker.Application.Search;

public partial class CustSearchAppService
{
    private async Task<long> AdminSiteAsync()
    {
        if (await loginUserData.GetUserId() == 0) throw new UnauthorizedAccessException();
        var site = await loginUserData.GetWebsiteId();
        if (site <= 0) throw new UnauthorizedAccessException();
        return site;
    }

    // One availability rule for admin options and the existing public search menu.
    private async Task<bool> HasSearchableProductsAsync(long site)
    {
        if (!await db.Websites.AsNoTracking().AnyAsync(x => x.Id == site && !x.IsDeleted && (int)x.Level >= 1)) return false;
        var now = DateTime.Now;
        return await db.Prods.AsNoTracking().AnyAsync(x => x.FK_WebsiteId == site && !x.IsDeleted &&
            x.Visible && !x.RemovedFromShelves && (x.permanent ||
                (x.StartTime.HasValue && x.EndTime.HasValue && x.StartTime.Value <= now && x.EndTime.Value >= now)));
    }

    private static bool HasAvailableScope(CustSearchEditDto item, bool hasProducts) =>
        (hasProducts && (item.SearchAllProd || item.ProductTagIds.Count > 0)) ||
        item.SearchAllArticle || item.ArticleTagIds.Count > 0 || item.SearchAllMenu || item.MenuIds.Count > 0;

    private static CustSearchEditDto BuiltIn(long id) => new()
    {
        Id = id, IsSystem = true, Title = id == SearchTargetIds.Product ? "商品" : "文章",
        Placeholder = id == SearchTargetIds.Product ? "請輸入商品名稱／型號" : "請輸入文章關鍵字",
        SerNO = id == SearchTargetIds.Product ? 0 : 1,
        SearchAllProd = id == SearchTargetIds.Product, SearchAllArticle = id == SearchTargetIds.Article
    };

    private static CustSearchEditDto ToEdit(CustSearch row)
    {
        if (row.SystemTargetId.HasValue)
        {
            var system = BuiltIn(row.SystemTargetId.Value);
            system.IsDefault = row.IsDefault;
            return system;
        }
        var scope = string.IsNullOrWhiteSpace(row.ScopeJson) ? new CustSearchScopeDto() :
            JsonConvert.DeserializeObject<CustSearchScopeDto>(row.ScopeJson) ?? new();
        return new CustSearchEditDto
        {
            Id = row.SystemTargetId ?? row.Id, IsSystem = row.SystemTargetId.HasValue,
            Title = row.Title ?? "", Description = row.Description, Placeholder = row.Placeholder,
            SerNO = row.SerNo, Visible = row.Visible, IsDefault = row.IsDefault,
            SearchAllProd = row.SearchAllProd, SearchAllArticle = row.SearchAllArticle, SearchAllMenu = row.SearchAllMenu,
            ProductTagIds = scope.ProductTagIds ?? new(), ArticleTagIds = scope.ArticleTagIds ?? new(), MenuIds = scope.MenuIds ?? new()
        };
    }

    private async Task<List<CustSearchEditDto>> AdminItemsAsync(long site)
    {
        var hasProducts = await HasSearchableProductsAsync(site);
        var rows = await db.CustSearch.AsNoTracking().Where(x => !x.IsDeleted && x.FK_WebsiteId == site).ToListAsync();
        var items = rows.Where(x => hasProducts || x.SystemTargetId != SearchTargetIds.Product).Select(ToEdit).ToList();
        var builtIns = hasProducts ? new[] { SearchTargetIds.Product, SearchTargetIds.Article } : new[] { SearchTargetIds.Article };
        foreach (var id in builtIns)
            if (!items.Any(x => x.Id == id)) items.Add(BuiltIn(id));
        foreach (var item in items) item.IsAvailable = HasAvailableScope(item, hasProducts);
        var effectiveDefault = items.FirstOrDefault(x => x.Visible && x.IsAvailable && x.IsDefault)
            ?? items.FirstOrDefault(x => x.Visible && x.IsAvailable && x.Id == (hasProducts ? SearchTargetIds.Product : SearchTargetIds.Article))
            ?? items.Where(x => x.Visible && x.IsAvailable).OrderBy(x => x.SerNO).ThenBy(x => x.Id).FirstOrDefault();
        // Availability changes affect the effective selection without deleting or rewriting saved settings.
        foreach (var item in items) item.IsDefault = ReferenceEquals(item, effectiveDefault);
        return items.OrderByDescending(x => x.IsSystem).ThenBy(x => x.SerNO).ThenBy(x => x.Id).ToList();
    }

    private static string ScopeSummary(CustSearchEditDto x) => string.Join("、", new[]
    {
        x.SearchAllProd ? "全部商品" : x.ProductTagIds.Count > 0 ? $"商品標籤 {x.ProductTagIds.Count} 項" : null,
        x.SearchAllArticle ? "全部文章" : x.ArticleTagIds.Count > 0 ? $"文章標籤 {x.ArticleTagIds.Count} 項" : null,
        x.SearchAllMenu ? "全部選單" : x.MenuIds.Count > 0 ? $"指定選單 {x.MenuIds.Count} 項" : null
    }.Where(x => x != null));

    public async Task<JsonResult> GetAll(DataSourceLoadOptions loadOptions)
    {
        var items = await AdminItemsAsync(await AdminSiteAsync());
        var rows = items.Select(x => new CuseSearchListDto
        {
            Id = x.Id, Title = x.Title, Placeholder = x.Placeholder ?? "", Visible = x.Visible,
            SerNO = x.SerNO, IsDefault = x.IsDefault, IsSystem = x.IsSystem, IsAvailable = x.IsAvailable, ScopeSummary = ScopeSummary(x)
        });
        // Apply default ordering in data loading, without exposing grid sort badges in the UI.
        if (loadOptions.Sort == null || loadOptions.Sort.Length == 0)
            loadOptions.Sort = new[]
            {
                new SortingInfo { Selector = "IsSystem", Desc = true },
                new SortingInfo { Selector = "SerNO", Desc = false },
                new SortingInfo { Selector = "Id", Desc = false }
            };
        return new JsonResult(DataSourceLoader.Load(rows, loadOptions),
            new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
    }

    public async Task<CustSearchEditDto?> GetDataOne(long id)
    {
        var site = await AdminSiteAsync();
        var item = (await AdminItemsAsync(site)).FirstOrDefault(x => x.Id == id);
        if (item == null) return null;
        var ids = item.ProductTagIds.Concat(item.ArticleTagIds).Distinct().ToList();
        item.SelectedTags = await db.Tags.AsNoTracking()
            .Where(x => x.FK_WebsiteId == site && !x.IsDeleted && ids.Contains(x.Id))
            .Select(x => new CustSearchOptionDto { Id = x.Id, Title = x.Title ?? "" }).ToListAsync();
        return item;
    }

    public async Task<CustSearchOptionsDto> GetOptions()
    {
        var site = await AdminSiteAsync();
        return new()
        {
            HasProducts = await HasSearchableProductsAsync(site),
            Menus = await db.WebMenus.AsNoTracking().Where(x => x.FK_WebsiteId == site && !x.IsDeleted && !x.RemovedFromShelves)
                .OrderBy(x => x.Title).Select(x => new CustSearchOptionDto { Id = x.Id, Title = x.Title ?? "" }).ToListAsync()
        };
    }

    public async Task<ResponseMessageDto> SetDefault(long id)
    {
        var site = await AdminSiteAsync();
        var system = id == SearchTargetIds.Product || id == SearchTargetIds.Article;
        if (id <= 0 && !system) return new() { Message = "無效的搜尋項目。" };
        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        try
        {
            var row = system
                ? await db.CustSearch.SingleOrDefaultAsync(x => !x.IsDeleted && x.FK_WebsiteId == site && x.SystemTargetId == id)
                : await db.CustSearch.SingleOrDefaultAsync(x => !x.IsDeleted && x.FK_WebsiteId == site && x.Id == id && x.SystemTargetId == null);
            if (!system && row == null) return new() { Message = "找不到目前站台的搜尋項目。" };
            if (!system && !row!.Visible) return new() { Message = "請先啟用此搜尋項目，再設為預設。" };
            var hasProducts = await HasSearchableProductsAsync(site);
            if (id == SearchTargetIds.Product && !hasProducts)
                return new() { Message = "目前沒有可見且有效上架的商品，無法設定商品搜尋。" };
            if (!system && !HasAvailableScope(ToEdit(row!), hasProducts))
                return new() { Message = "此搜尋項目目前沒有可用範圍，請選擇其他項目。" };
            var previous = await db.CustSearch.Where(x => !x.IsDeleted && x.FK_WebsiteId == site && x.IsDefault).ToListAsync();
            previous.ForEach(x => x.IsDefault = false);
            await loginUserData.SaveChanges(previous.Cast<object>());
            if (row == null)
            {
                var item = BuiltIn(id);
                row = new CustSearch { FK_WebsiteId = site, SystemTargetId = id, Title = item.Title,
                    Placeholder = item.Placeholder ?? "", SerNo = item.SerNO, Visible = true,
                    SearchAllProd = item.SearchAllProd, SearchAllArticle = item.SearchAllArticle };
                db.CustSearch.Add(row);
            }
            // Only the default flag changes for an existing custom item; never rewrite its scope from a stale form.
            row.IsDefault = true;
            if (system) row.Visible = true;
            await loginUserData.SaveChanges(row);
            if (transaction != null) await transaction.CommitAsync();
            return new() { Success = true };
        }
        catch (DbUpdateException) { return new() { Message = "搜尋設定已變動，請重新載入後再試。" }; }
    }

    public async Task<ResponseMessageDto> AddUp(CustSearchEditDto dto)
    {
        var site = await AdminSiteAsync();
        var title = dto.Title?.Trim() ?? "";
        if (title.Length == 0 || title.Length > 60 || (dto.Description?.Length ?? 0) > 150 ||
            (dto.Placeholder?.Length ?? 0) > 100 || dto.SerNO < 0)
            return new() { Message = "請確認名稱、文字長度與排序。" };
        if (dto.Id < 0 && dto.Id != SearchTargetIds.Product && dto.Id != SearchTargetIds.Article)
            return new() { Message = "無效的搜尋項目。" };

        var system = dto.Id == SearchTargetIds.Product || dto.Id == SearchTargetIds.Article;
        if (system) return new() { Message = "商品與文章為固定項目，只能在列表選擇預設。" };
        var scope = new CustSearchScopeDto
        {
            ProductTagIds = (dto.ProductTagIds ?? new()).Distinct().ToList(),
            ArticleTagIds = (dto.ArticleTagIds ?? new()).Distinct().ToList(),
            MenuIds = (dto.MenuIds ?? new()).Distinct().ToList()
        };
        // Validate every supplied reference, even if its all-content switch is on.
        var tags = scope.ProductTagIds.Concat(scope.ArticleTagIds).Distinct().ToList();
        if (tags.Count > 1000 || scope.MenuIds.Count > 1000 || tags.Any(x => x <= 0) || scope.MenuIds.Any(x => x <= 0) ||
            await db.Tags.CountAsync(x => x.FK_WebsiteId == site && !x.IsDeleted && tags.Contains(x.Id)) != tags.Count ||
            await db.WebMenus.CountAsync(x => x.FK_WebsiteId == site && !x.IsDeleted && !x.RemovedFromShelves && scope.MenuIds.Contains(x.Id)) != scope.MenuIds.Count)
            return new() { Message = "標籤或選單已失效，請重新選擇目前站台的資料。" };
        if (dto.SearchAllProd) scope.ProductTagIds.Clear();
        if (dto.SearchAllArticle) scope.ArticleTagIds.Clear();
        if (dto.SearchAllMenu) scope.MenuIds.Clear();
        if (!dto.SearchAllProd && !dto.SearchAllArticle && !dto.SearchAllMenu &&
            scope.ProductTagIds.Count == 0 && scope.ArticleTagIds.Count == 0 && scope.MenuIds.Count == 0)
            return new() { Message = "請至少設定一種搜尋範圍。" };

        await using var transaction = db.Database.IsRelational()
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        try
        {
            CustSearch? row = dto.Id > 0
                ? await db.CustSearch.SingleOrDefaultAsync(x => !x.IsDeleted && x.FK_WebsiteId == site && x.Id == dto.Id && x.SystemTargetId == null) : null;
            if (dto.Id > 0 && row == null) return new() { Message = "找不到目前站台的搜尋項目。" };
            if (!await HasSearchableProductsAsync(site) && (dto.SearchAllProd || scope.ProductTagIds.Count > 0))
            {
                var old = row == null ? null : ToEdit(row);
                if (old == null || old.SearchAllProd != dto.SearchAllProd ||
                    !old.ProductTagIds.ToHashSet().SetEquals(scope.ProductTagIds))
                    return new() { Message = "目前沒有可見且有效上架的商品，暫不提供新增或變更商品範圍。" };
            }
            dto.IsDefault = row?.IsDefault ?? false;
            if (dto.IsDefault && !dto.Visible) return new() { Message = "請先在列表改選其他預設，再隱藏此項目。" };
            if (row == null)
            {
                row = new CustSearch { FK_WebsiteId = site };
                db.CustSearch.Add(row);
            }
            row.Title = title; row.Description = dto.Description?.Trim(); row.Placeholder = dto.Placeholder?.Trim() ?? "";
            row.SerNo = dto.SerNO; row.Visible = dto.Visible; row.IsDefault = dto.IsDefault;
            row.SearchAllProd = dto.SearchAllProd; row.SearchAllArticle = dto.SearchAllArticle; row.SearchAllMenu = dto.SearchAllMenu;
            row.ScopeJson = JsonConvert.SerializeObject(scope);
            await loginUserData.SaveChanges(row);
            if (transaction != null) await transaction.CommitAsync();
            return new() { Success = true, Object = row.SystemTargetId ?? row.Id };
        }
        catch (DbUpdateException) { return new() { Message = "搜尋設定已變動，請重新載入後再儲存。" }; }
    }

    public async Task<ResponseMessageDto> Delete(long id)
    {
        var site = await AdminSiteAsync();
        if (id <= 0) return new() { Message = "商品與文章為固定項目，無法刪除；只能在列表選擇預設。" };
        var row = await db.CustSearch.SingleOrDefaultAsync(x => !x.IsDeleted && x.FK_WebsiteId == site && x.Id == id && x.SystemTargetId == null);
        if (row == null) return new() { Message = "找不到目前站台的搜尋項目。" };
        row.IsDefault = false; row.IsDeleted = true;
        await loginUserData.SaveChanges(row);
        return new() { Success = true };
    }
}
