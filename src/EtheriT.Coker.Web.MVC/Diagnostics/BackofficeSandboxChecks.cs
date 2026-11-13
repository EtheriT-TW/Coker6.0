using AutoMapper;
using DevExtreme.AspNet.Mvc;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Common;
using EtheriT.Coker.Application.Shared.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.Specification;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static readonly IReadOnlyDictionary<string, string> Pages = new Dictionary<string, string>
    {
        ["sandbox/diagnostics-history"] = "SystemDiagnostics.Backoffice",
        ["sandbox/diagnostics-guards"] = "SystemDiagnostics.Backoffice",
        ["sandbox/public-diagnostics"] = "SystemDiagnostics.Public",
        ["sandbox/public-functions"] = "SystemDiagnostics.Functions",
        ["sandbox/public-commerce/quickCart"] = "SystemDiagnostics.Functions",
        ["sandbox/public-commerce/addOn"] = "SystemDiagnostics.Functions",
        ["sandbox/public-commerce/cart"] = "SystemDiagnostics.Functions",
        ["sandbox/public-commerce/bonus"] = "SystemDiagnostics.Functions",
        ["sandbox/public-commerce/payment"] = "SystemDiagnostics.Functions",
        ["sandbox/diagnostics-background"] = "SystemDiagnostics.Backoffice",
        ["sandbox/product-core"] = "ProductManagement.ProductList",
        ["sandbox/product-stock"] = "ProductManagement.ProductList",
        ["sandbox/product-prices"] = "ProductManagement.ProductList",
        ["sandbox/product-batch-stock"] = "ProductManagement.ProductList",
        ["sandbox/product-delete"] = "ProductManagement.ProductList",
        ["sandbox/product-scope"] = "ProductManagement.ProductList",
        ["sandbox/stock-alerts"] = "ProductManagement.SaleQuantityStaging",
        ["sandbox/spec-type"] = "ProductManagement.SpecSetting",
        ["sandbox/spec-value"] = "ProductManagement.SpecSetting",
        ["sandbox/spec-delete"] = "ProductManagement.SpecSetting",
        ["sandbox/marquee-core"] = "ContentManagement.Marquee",
        ["sandbox/marquee-delete"] = "ContentManagement.Marquee"
        , ["sandbox/certificate-crud"] = "ProductManagement.TechnicalCertificate"
        , ["sandbox/directory-crud"] = "ContentManagement.Directory"
        , ["sandbox/directory-all-content"] = "Page.Index"
        , ["sandbox/menu-content-cache"] = "Page.Index"
        , ["sandbox/article-crud"] = "ContentManagement.Article"
        , ["sandbox/enter-ad-crud"] = "AdvertisementManagement.EnterAd"
        , ["sandbox/side-ad-crud"] = "AdvertisementManagement.RightSideAd"
        , ["sandbox/tag-crud"] = "ContentManagement.Tag"
        , ["sandbox/tag-group-crud"] = "ContentManagement.Tag"
        , ["sandbox/user-group-crud"] = "MemberManagement.UserType"
        , ["sandbox/member-role-crud"] = "MemberManagement.MemberType"
        , ["sandbox/box-crud"] = "StoreSettings.LogisticsBox"
        , ["sandbox/template-global"] = "Template.GlobalSettings"
        , ["sandbox/template-header"] = "Template.HeaderSettings"
        , ["sandbox/template-footer"] = "Template.FooterSettings"
        , ["sandbox/contact-status"] = "ContentManagement.ContactUs"
        , ["sandbox/contact-reply"] = "ContentManagement.ContactUs"
        , ["sandbox/mail-envelope"] = "StoreSettings.SMTPSettings"
        , ["sandbox/file-recycle"] = "FileManagement.Index"
        , ["sandbox/file-path"] = "FileManagement.Index"
    };

    public static bool Supports(string page, string? executor) =>
        executor != null && Pages.TryGetValue(executor, out var expected) && expected == page;

    public static async Task<BackofficeCheckOutcome?> ExecuteAsync(string page, string? executor,
        BackofficeSandboxFactory factory, IMapper mapper, WebsiteLevelEnum level, CancellationToken token)
    {
        if (executor == null || !Pages.TryGetValue(executor, out var expected) || page != expected) return null;
        if (executor.StartsWith("sandbox/public-commerce/", StringComparison.Ordinal))
        {
            var result = await ExecutePublicFeatureAsync(executor["sandbox/public-commerce/".Length..], factory, mapper, level, token);
            return new(result.Status == PublicCheckStatus.Passed ? BackofficeTestStatus.Passed : BackofficeTestStatus.Failed, result.Message);
        }
        using var effects = new DiagnosticsSandboxScope();
        await using var sandbox = await factory.CreateAsync(mapper, level, token);
        token.ThrowIfCancellationRequested();
        switch (executor)
        {
            case "sandbox/diagnostics-history": await DiagnosticsHistory(sandbox); break;
            case "sandbox/diagnostics-guards": await DiagnosticsGuards(sandbox); break;
            case "sandbox/public-diagnostics": await PublicDiagnosticsRegression(sandbox); break;
            case "sandbox/public-functions": await PublicFunctionsRegression(sandbox); break;
            case "sandbox/diagnostics-background": await DiagnosticsBackgroundRegression(sandbox); break;
            case "sandbox/product-core": await ProductCore(sandbox); break;
            case "sandbox/product-stock": await ProductStock(sandbox); break;
            case "sandbox/product-prices": await ProductPrices(sandbox); break;
            case "sandbox/product-batch-stock": await ProductBatch(sandbox); break;
            case "sandbox/product-delete": await ProductDelete(sandbox); break;
            case "sandbox/product-scope": await ProductScope(sandbox); break;
            case "sandbox/stock-alerts": await StockAlerts(sandbox); break;
            case "sandbox/spec-type": await SpecType(sandbox); break;
            case "sandbox/spec-value": await SpecValue(sandbox); break;
            case "sandbox/spec-delete": await SpecDelete(sandbox); break;
            case "sandbox/marquee-core": await MarqueeCore(sandbox); break;
            case "sandbox/marquee-delete": await MarqueeDelete(sandbox); break;
            case "sandbox/certificate-crud": await CertificateCrud(sandbox); break;
            case "sandbox/directory-crud": await DirectoryCrud(sandbox); break;
            case "sandbox/directory-all-content": await DirectoryAllContents(sandbox); break;
            case "sandbox/menu-content-cache": await MenuPublishedContentCache(sandbox); break;
            case "sandbox/article-crud": await ArticleCrud(sandbox); break;
            case "sandbox/enter-ad-crud": await AdvertiseCrud(sandbox, 1); break;
            case "sandbox/side-ad-crud": await AdvertiseCrud(sandbox, 2); break;
            case "sandbox/tag-crud": await TagCrud(sandbox, false); break;
            case "sandbox/tag-group-crud": await TagCrud(sandbox, true); break;
            case "sandbox/user-group-crud": await UserGroupCrud(sandbox); break;
            case "sandbox/member-role-crud": await MemberRoleCrud(sandbox); break;
            case "sandbox/box-crud": await BoxCrud(sandbox); break;
            case "sandbox/template-global": await TemplateGlobal(sandbox); break;
            case "sandbox/template-header": await TemplateHeader(sandbox); break;
            case "sandbox/template-footer": await TemplateFooter(sandbox); break;
            case "sandbox/contact-status": await ContactStatus(sandbox); break;
            case "sandbox/contact-reply": await ContactReply(sandbox); break;
            case "sandbox/mail-envelope": await MailEnvelope(sandbox); break;
            case "sandbox/file-recycle": await FileRecycle(sandbox); break;
            case "sandbox/file-path": FilePathBoundary(sandbox); break;
            default: throw new InvalidOperationException("Missing sandbox workflow implementation.");
        }
        return new(BackofficeTestStatus.Passed, "隔離測試資料中的業務流程通過；不含瀏覽器操作及 SQL 相容性驗證。");
    }

    private static void Require(bool condition, string reason)
    { if (!condition) throw new InvalidOperationException("Diagnostic assertion failed: " + reason); }

    private static async Task DiagnosticsGuards(BackofficeSandbox box)
    {
        // Exercise the real interceptor without opening a connection or running SQL.
        var interceptor = new DiagnosticsCommandGuard();
        using (var session = new DiagnosticsReadOnlySession())
        {
            using var command = new Microsoft.Data.SqlClient.SqlCommand(
                "SELECT [Id] FROM [Prods] ORDER BY [Id] OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY");
            interceptor.ScalarExecuting(command, null!, default);
            Require(!session.WriteAttempted, "Read-only pagination was rejected.");
        }
        foreach (var sql in new[] { "SELECT NEXT VALUE FOR dbo.Sequence", "SELECT 1; UPDATE Prods SET Title = 'changed'",
            "SELECT * INTO NewTable FROM Prods", "DELETE FROM Prods", "EXEC dbo.Maintenance" })
        {
            using var session = new DiagnosticsReadOnlySession();
            using var command = new Microsoft.Data.SqlClient.SqlCommand(sql);
            try { interceptor.ScalarExecuting(command, null!, default); }
            catch (InvalidOperationException) when (session.WriteAttempted) { }
            Require(session.WriteAttempted, "Unsafe command was accepted: " + sql);
        }
        // Model creation must still register domain entities and apply soft deletion.
        Require(box.Db.Model.FindEntityType(typeof(TechnicalCertificate)) != null, "Certificate model was not registered.");
        var deleted = new Prod { Title = "檢測軟刪除", Introduction = "", Description = "", FK_WebsiteId = 999000001, IsDeleted = true };
        box.Db.Prods.Add(deleted);
        await box.Db.SaveChangesAsync();
        var id = deleted.Id;
        box.Db.ChangeTracker.Clear();
        Require(!await box.Db.Prods.AnyAsync(row => row.Id == id)
            && await box.Db.Prods.IgnoreQueryFilters().AnyAsync(row => row.Id == id), "Domain soft deletion configuration was lost.");
    }

    private static async Task DiagnosticsHistory(BackofficeSandbox box)
    {
        var root = box.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
        var first = new BackofficeTestPage("first", "fixture", "fixture", "fixture", "fixture", new(), new()
        {
            new BackofficeTestCase("case", "fixture", "fixture", "fixture", "isolated") { Status = BackofficeTestStatus.Failed }
        }) { BasicStatus = BackofficeTestStatus.Passed };
        var store = new BackofficeResultStore(root);
        await store.SaveAsync(1, 1, "scope", new[] { first }, new Dictionary<string, string> { ["case"] = "fixture exception" });
        // A fresh instance has no shared memory cache, just as after a process restart.
        store = new BackofficeResultStore(root);
        var restored = await store.LoadAsync(1, 1, "scope");
        Require(restored?.Pages["first"].Cases[0].Status == BackofficeTestStatus.Failed, "Stored failure was not restored.");
        Require(restored?.Errors.GetValueOrDefault("case") == "fixture exception", "Internal error evidence was not restored.");
        Require(await store.LoadAsync(2, 1, "scope") == null && await store.LoadAsync(1, 2, "scope") == null,
            "History leaked across site or user.");
        Require(await store.LoadAsync(1, 1, "changed") == null, "Stale scope was restored.");
        var latest = await store.LoadAsync(1, 1, null);
        Require(latest?.ScopeStamp == "scope" && latest.AtUtc != default,
            "History could not read the last record timestamp after a scope change.");
        Require(await store.LoadAsync(2, 1, null) == null && await store.LoadAsync(1, 2, null) == null,
            "Latest history leaked across site or user.");
        var second = first with { Id = "second", Cases = new() { first.Cases[0] with { Id = "second-case" } } };
        await store.SaveAsync(1, 1, "scope", new[] { second }, new Dictionary<string, string> { ["second-case"] = "other exception" });
        await store.SaveAsync(1, 1, "scope", new[] { first with { Cases = new() { first.Cases[0] with { Status = BackofficeTestStatus.Passed } } } });
        restored = await new BackofficeResultStore(root).LoadAsync(1, 1, "scope");
        Require(restored?.Pages.Count == 2 && restored.Pages["first"].Cases[0].Status == BackofficeTestStatus.Passed
            && restored.Pages["second"].Cases[0].Status == BackofficeTestStatus.Failed, "Retest did not replace only its page.");
        Require(restored != null && !restored.Errors.ContainsKey("case")
            && restored.Errors.GetValueOrDefault("second-case") == "other exception", "Retest left stale evidence or cleared another page's error.");
    }
    private static void Success(ResponseMessageDto result) => Require(result.Success,
        !string.IsNullOrWhiteSpace(result.Error) ? result.Error : result.Message ?? "Service failed.");
    private static IProductAppService Products(BackofficeSandbox box) => box.Services.GetRequiredService<IProductAppService>();

    private static ProdAddUpDto ProductFixture() => new()
    {
        Title = "檢測商品", ItemNo = "DIAGNOSTICS", Description = "檢測說明", Introduction = "檢測簡介",
        Visible = true, Permanent = true, status = ProdStatusEnum.一般, TagSelected = new(), TechCertSelected = new(),
        Stocks = new() { new ProductStockDto { Stock = 20, OldStock = 20, Min_Qty = 1, Alert_Qty = 5,
            Price = 100, SubItemNo = "DIAGNOSTICS-SKU", PackingPoint = 1, Prices = new() } }
    };
    private static async Task<ProdAddUpDto> CreateProduct(BackofficeSandbox box, ProdAddUpDto? seed = null)
    {
        var input = seed ?? ProductFixture();
        var response = await Products(box).ProductAddUp(input);
        Require(response.Success, "Product creation failed: " +
            (!string.IsNullOrWhiteSpace(response.Error) ? response.Error : response.Message));
        Require(long.TryParse(response.Message, out var id) && id > 0, "Product ID was not returned.");
        input.Id = id;
        box.Db.ChangeTracker.Clear();
        var rows = await box.Db.Prod_Stocks.AsNoTracking().Where(stock => stock.FK_Pid == id && !stock.IsDeleted).ToListAsync();
        Require(rows.Count == 1 && rows[0].Stock == 20, "Initial stock was not persisted.");
        input.Stocks[0].Id = rows[0].Id;
        return input;
    }

    private static async Task ProductCore(BackofficeSandbox box)
    {
        var service = Products(box);
        var invalid = ProductFixture(); invalid.Title = " ";
        Require(!(await service.ProductAddUp(invalid)).Success, "Blank title was accepted.");
        var input = await CreateProduct(box);
        input.Title = "修改後商品"; input.Visible = false; input.RemovedFromShelves = true;
        Success(await service.ProductAddUp(input));
        box.Db.ChangeTracker.Clear();
        var saved = await box.Db.Prods.AsNoTracking().SingleAsync(row => row.Id == input.Id);
        Require(saved.Title == input.Title && !saved.Visible && saved.RemovedFromShelves, "Product edit was not persisted.");
    }
    private static async Task ProductStock(BackofficeSandbox box)
    {
        var input = await CreateProduct(box);
        input.Stocks[0].Stock = 12; input.Stocks[0].Min_Qty = 2; input.Stocks[0].Alert_Qty = 6;
        input.Stocks[0].PackingPoint = 3; input.Stocks[0].SubItemNo = "UPDATED-SKU";
        Success(await Products(box).ProductAddUp(input));
        box.Db.ChangeTracker.Clear();
        var stock = await box.Db.Prod_Stocks.AsNoTracking().SingleAsync(row => row.Id == input.Stocks[0].Id);
        Require(stock.Stock == 12 && stock.Min_Qty == 2 && stock.Alert_Qty == 6 && stock.PackingPoint == 3 && stock.SubItemNo == "UPDATED-SKU", "Stock edit mismatch.");
    }
    private static async Task ProductPrices(BackofficeSandbox box)
    {
        var input = await CreateProduct(box);
        var role = new Role { Name = "檢測會員價", FK_WebsiteId = 999000001, Type = RoleTypeEnum.前台 };
        box.Db.Roles.Add(role); await box.Db.SaveChangesAsync();
        var price = new ProductPriceDto { FK_PSId = input.Stocks[0].Id, FK_RId = role.Id, Price = 88, Bonus = 3 };
        Success(await Products(box).PriceAddUp(new() { price }));
        box.Db.ChangeTracker.Clear();
        var saved = await box.Db.Prod_Prices.AsNoTracking().SingleAsync(row => row.FK_PSId == price.FK_PSId && row.FK_RId == role.Id);
        Require(saved.Price == 88 && saved.Bonus == 3, "Member price mismatch.");
        price.Id = saved.Id; price.Price = 75;
        Success(await Products(box).PriceAddUp(new() { price }));
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.Prod_Prices.AsNoTracking().SingleAsync(row => row.Id == saved.Id)).Price == 75, "Price update mismatch.");
    }
    private static async Task ProductBatch(BackofficeSandbox box)
    {
        var first = await CreateProduct(box); var second = await CreateProduct(box);
        Success(await Products(box).StockBatchSet(new() { new StockBatchSetDto { Id = first.Stocks[0].Id, StockQuantity = 7 } }));
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.Prod_Stocks.AsNoTracking().SingleAsync(row => row.Id == first.Stocks[0].Id)).Stock == 7, "Selected stock was not updated.");
        Require((await box.Db.Prod_Stocks.AsNoTracking().SingleAsync(row => row.Id == second.Stocks[0].Id)).Stock == 20, "Unselected stock was changed.");
    }
    private static async Task ProductDelete(BackofficeSandbox box)
    {
        var input = await CreateProduct(box);
        Success(await Products(box).ProdDelete(input.Id)); box.Db.ChangeTracker.Clear();
        Require((await box.Db.Prods.IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == input.Id)).IsDeleted, "Product was not deleted.");
        Require((await box.Db.Prod_Stocks.IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == input.Stocks[0].Id)).IsDeleted, "Associated stock was not deleted.");
    }
    private static async Task ProductScope(BackofficeSandbox box)
    {
        var input = await CreateProduct(box);
        var product = await box.Db.Prods.SingleAsync(row => row.Id == input.Id);
        product.FK_WebsiteId = 999000002; await box.Db.SaveChangesAsync(); box.Db.ChangeTracker.Clear();
        input.Title = "不應被更新";
        Require(!(await Products(box).ProductAddUp(input)).Success, "Cross-site product edit was accepted.");
        Require(!(await Products(box).ProdDelete(input.Id)).Success, "Cross-site deletion was accepted.");
        box.Db.ChangeTracker.Clear();
        var saved = await box.Db.Prods.IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == input.Id);
        Require(saved.Title == "檢測商品" && !saved.IsDeleted, "Cross-site data was modified.");
    }
    private static async Task StockAlerts(BackofficeSandbox box)
    {
        var input = await CreateProduct(box);
        foreach (var quantity in new[] { 4, 5, 6 })
        {
            input.Stocks[0].Stock = quantity;
            Success(await Products(box).ProductAddUp(input));
            var result = await Products(box).SaleQuantityStaging(new DataSourceLoadOptions { Take = 10 });
            var json = JToken.FromObject(result.Value!);
            Require(json["data"] is JArray, "Stock alert query failed.");
            var found = ((JArray)json["data"]!).Any(row => row["Id"]?.Value<long>() == input.Stocks[0].Id);
            Require(found == (quantity <= 5), "Stock alert boundary mismatch.");
        }
    }
    private static async Task<long> CreateSpecType(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<ISpecificationAppService>();
        Success(await service.TypeAddUp(new DevExpressDto { Values = JsonConvert.SerializeObject(new { Type = "檢測規格" }) }));
        box.Db.ChangeTracker.Clear();
        return (await box.Db.Prod_Spec_Types.AsNoTracking().SingleAsync(row => row.FK_WebsiteId == 999000001 && row.Type == "檢測規格")).Id;
    }
    private static async Task SpecType(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<ISpecificationAppService>();
        Require(!(await service.TypeAddUp(new DevExpressDto { Values = "{\"Type\":\" \"}" })).Success, "Empty type was accepted.");
        var id = await CreateSpecType(box);
        Success(await service.TypeAddUp(new DevExpressDto { Key = checked((int)id), Values = "{\"Type\":\"更新規格\"}" }));
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.Prod_Spec_Types.AsNoTracking().SingleAsync(row => row.Id == id)).Type == "更新規格", "Type update mismatch.");
    }
    private static async Task SpecValue(BackofficeSandbox box)
    {
        var id = await CreateSpecType(box);
        var service = box.Services.GetRequiredService<ISpecificationAppService>();
        Success(await service.SpecAddUp(new SpecSpecListDto { FK_Tid = id, Title = "檢測值" }));
        box.Db.ChangeTracker.Clear();
        var saved = await box.Db.Prod_Specs.AsNoTracking().SingleAsync(row => row.FK_Tid == id && row.Title == "檢測值");
        Success(await service.SpecAddUp(new SpecSpecListDto { Id = saved.Id, FK_Tid = id, Title = "更新值" }));
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.Prod_Specs.AsNoTracking().SingleAsync(row => row.Id == saved.Id)).Title == "更新值", "Spec value update mismatch.");
    }
    private static async Task SpecDelete(BackofficeSandbox box)
    {
        var id = await CreateSpecType(box);
        Success(await box.Services.GetRequiredService<ISpecificationAppService>().TypeDelete(id));
        box.Db.ChangeTracker.Clear();
        Require((await box.Db.Prod_Spec_Types.IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == id)).IsDeleted, "Spec type was not deleted.");
    }
    private static MarqueeDto MarqueeFixture() => new() { title = "檢測跑馬燈", placement = "Top", disp_opt = true,
        permanent = true, ser_no = 1, link = "https://example.invalid", target = false };
    private static async Task MarqueeCore(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IMarqueeAppService>(); var input = MarqueeFixture();
        Success(await service.AddUp(input)); box.Db.ChangeTracker.Clear();
        input.Id = (await box.Db.Marquees.AsNoTracking().SingleAsync(row => row.FK_WebsiteId == 999000001 && row.title == input.title)).Id;
        input.title = "更新跑馬燈"; input.placement = "Inner"; input.target = true;
        Success(await service.AddUp(input)); box.Db.ChangeTracker.Clear();
        var saved = await box.Db.Marquees.AsNoTracking().SingleAsync(row => row.Id == input.Id);
        Require(saved.title == input.title && saved.placement == input.placement && saved.target, "Marquee update mismatch.");
    }
    private static async Task MarqueeDelete(BackofficeSandbox box)
    {
        var service = box.Services.GetRequiredService<IMarqueeAppService>(); var input = MarqueeFixture();
        Success(await service.AddUp(input)); box.Db.ChangeTracker.Clear();
        var saved = await box.Db.Marquees.AsNoTracking().SingleAsync(row => row.FK_WebsiteId == 999000001 && row.title == input.title);
        Success(await service.Delete(checked((int)saved.Id))); box.Db.ChangeTracker.Clear();
        Require((await box.Db.Marquees.IgnoreQueryFilters().AsNoTracking().SingleAsync(row => row.Id == saved.Id)).IsDeleted, "Marquee was not deleted.");
    }
}
