using DevExtreme.AspNet.Mvc;
using DevExtreme.AspNet.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using EtheriT.Coker.Application.Shared.Product;
using EtheriT.Coker.Application.Shared.Specification;
using EtheriT.Coker.Application.Shared.TechnicalCertificate;
using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Directory;
using EtheriT.Coker.Application.Shared.Article;
using EtheriT.Coker.Application.Shared.Advertise;
using EtheriT.Coker.Application.Shared.Member;
using EtheriT.Coker.Application.Shared.UserHabits;
using EtheriT.Coker.Application.Shared.Freight;
using EtheriT.Coker.Application.Shared.Marketing;
using EtheriT.Coker.Application.Newsletter;
using EtheriT.Coker.Application.Search;
using EtheriT.Coker.Application.Shared.Marquee;
using EtheriT.Coker.Application.Shared.Tag;
using EtheriT.Coker.Application.AuditLog;
using EtheriT.Coker.Application.Shared.Remote;
using EtheriT.Coker.Application.Shared.FlowSize;
using EtheriT.Coker.Application.Shared.BonusManagement;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Contact;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public sealed record BackofficeCheckOutcome(BackofficeTestStatus Status, string Message);

// Only reviewed query methods are callable. The catalog cannot supply arbitrary
// URLs, service types, reflection methods or SQL statements.
public static class BackofficeReadOnlyChecks
{
    private sealed record ListDefinition(string Page, string Key, string[] Fields, bool AllowEmptyArray = false);
    private static readonly IReadOnlyDictionary<string, ListDefinition> Lists = new Dictionary<string, ListDefinition>
    {
        ["orders"] = new("OrderManagement.Index", "Id", new[] { "State", "Total" }),
        ["contacts"] = new("ContentManagement.ContactUs", "Id", new[] { "Status", "CreationTime" }, true),
        ["products"] = new("ProductManagement.ProductList", "Id", new[] { "Title", "Price", "TagNames", "StockDisplay" }),
        ["stock-alerts"] = new("ProductManagement.SaleQuantityStaging", "Id", new[] { "ProductId", "SaleQuantity", "StockQuantity" }),
        ["spec-types"] = new("ProductManagement.SpecSetting", "Id", new[] { "Type", "SeoVariantProperty" }),
        ["specs"] = new("ProductManagement.SpecSetting", "Id", new[] { "FK_Tid", "Title" }),
        ["certificates"] = new("ProductManagement.TechnicalCertificate", "Id", new[] { "Title", "Img" }),
        ["menus"] = new("Page.Index", "Id", new[] { "Title", "Link" }),
        ["directories"] = new("ContentManagement.Directory", "Id", new[] { "Title" }),
        ["articles"] = new("ContentManagement.Article", "Id", new[] { "Title" }),
        ["advertise-directories"] = new("AdvertisementManagement.CustomAd", "Id", new[] { "Title" }),
        ["enter-ads"] = new("AdvertisementManagement.EnterAd", "Id", new[] { "Title" }),
        ["side-ads"] = new("AdvertisementManagement.RightSideAd", "Id", new[] { "Title" }),
        ["members"] = new("MemberManagement.MemberList", "Id", new[] { "Name" }),
        ["member-roles"] = new("MemberManagement.MemberType", "Id", new[] { "Name" }),
        ["managers"] = new("MemberManagement.ManagerList", "Id", new[] { "Name" }),
        ["groups"] = new("MemberManagement.UserType", "Id", new[] { "Title" }),
        ["freight"] = new("StoreSettings.FreightSettings", "Id", new[] { "Title" }),
        ["boxes"] = new("StoreSettings.LogisticsBox", "Id", new[] { "Name" }),
        ["campaigns"] = new("StoreSettings.MarketingSettings", "Id", new[] { "Name" }),
        ["recipients"] = new("Newsletter.Recipient", "Id", Array.Empty<string>()),
        ["search"] = new("SearchManagement.CustSearch", "Id", new[] { "Title" }),
        ["marquee-list"] = new("ContentManagement.Marquee", "Id", new[] { "title", "placement" }),
        ["tag-group-list"] = new("ContentManagement.Tag", "Id", new[] { "Title" }),
        ["audit-list"] = new("AuditLogs.Index", "Id", Array.Empty<string>()),
        ["remote-page"] = new("Remote.Index", "date", new[] { "type", "name", "count" }),
        ["remote-site"] = new("Remote.Page", "date", new[] { "count", "MemCount" }),
        ["flow-day"] = new("FlowSize.Index", "ActionTime", new[] { "RequestSize", "ResponseSize", "Total" }),
        ["flow-month"] = new("FlowSize.Index", "ActionTime", new[] { "RequestSize", "ResponseSize", "Total" }),
        ["flow-year"] = new("FlowSize.Index", "ActionTime", new[] { "RequestSize", "ResponseSize", "Total" }),
        ["bonus-records"] = new("BonusManagement.Record", "ExecutionTime", new[] { "UUID", "Amount" }),
        ["bonus-members"] = new("BonusManagement.Transaction", "Id", new[] { "UUID", "Name" }),
    };

    public static bool Supports(string pageId, string? executor)
    {
        if (executor == null) return false;
        var parts = executor.Split('/');
        return parts.Length <= 2 && (parts.Length == 1 || parts[1] is "load" or "empty" or "filter" or "sort" or "paging") &&
            Lists.TryGetValue(parts[0], out var definition) && definition.Page == pageId;
    }

    public static async Task<BackofficeCheckOutcome?> ExecuteAsync(string pageId, string? executor,
        IServiceProvider services, CancellationToken cancellationToken, IDictionary<string, JArray>? samples = null)
    {
        if (!Supports(pageId, executor)) return null;
        var parts = executor!.Split('/');
        var key = parts[0];
        var operation = parts.Length == 1 ? "load" : parts[1];
        var definition = Lists[key];
        using var guard = new DiagnosticsReadOnlySession(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var options = Options(definition.Key);
        JArray baseline;
        if (samples != null && samples.TryGetValue(key, out var cached)) baseline = cached;
        else
        {
            baseline = Read(await Query(key, options, services), definition, guard);
            if (samples != null) samples[key] = baseline;
        }
        if (operation == "load") return Pass("列表查詢、資料欄位與回傳格式正常。");
        if (operation == "empty")
        {
            options = Options(definition.Key);
            options.Filter = new object[] { new object?[] { definition.Key, "=", null }, "and",
                new object?[] { definition.Key, "<>", null } };
            var empty = Read(await Query(key, options, services), definition, guard);
            return empty.Count == 0 ? Pass("無符合資料時正確回傳空列表。") : Fail("無資料條件的查詢結果不正確。");
        }
        if (baseline.Count == 0) return new(BackofficeTestStatus.Skipped, "目前沒有可驗證的資料；未將此項視為通過。");
        cancellationToken.ThrowIfCancellationRequested();
        if (operation == "filter")
        {
            var firstKey = baseline[0][definition.Key]!;
            options = Options(definition.Key);
            options.Filter = new object[] { definition.Key, "=", firstKey.ToObject<object>()! };
            var filtered = Read(await Query(key, options, services), definition, guard);
            return filtered.Count > 0 && filtered.All(row => JToken.DeepEquals(row[definition.Key], firstKey))
                ? Pass("篩選結果與指定資料一致。") : Fail("篩選條件未正確套用，或資料已在檢測期間變動。");
        }
        if (operation == "sort")
        {
            if (baseline.Count < 2) return new(BackofficeTestStatus.Skipped, "資料不足兩筆，無法驗證排序；未將此項視為通過。");
            options = Options(definition.Key);
            options.Sort = new[] { new SortingInfo { Selector = definition.Key, Desc = true } };
            var sorted = Read(await Query(key, options, services), definition, guard);
            return IsOrdered(baseline, definition.Key, false) && IsOrdered(sorted, definition.Key, true)
                ? Pass("遞增與遞減排序正確。") : Fail("排序結果不符合指定順序。");
        }
        if (operation == "paging")
        {
            if (baseline.Count < 2) return new(BackofficeTestStatus.Skipped, "資料不足兩筆，無法驗證分頁；未將此項視為通過。");
            options = Options(definition.Key);
            options.Skip = 1;
            options.Take = 1;
            var second = Read(await Query(key, options, services), definition, guard);
            return second.Count == 1 && JToken.DeepEquals(second[0][definition.Key], baseline[1][definition.Key])
                ? Pass("跳過筆數與分頁資料一致。") : Fail("分頁結果不一致，或資料已在檢測期間變動。");
        }
        return Fail("此檢測流程的設定不完整，請聯絡系統維護人員。");
    }

    private static DataSourceLoadOptions Options(string key) => new()
    {
        Take = 2, Skip = 0, RequireTotalCount = false,
        Sort = new[] { new SortingInfo { Selector = key, Desc = false } }
    };

    private static JArray Read(JsonResult result, ListDefinition definition, DiagnosticsReadOnlySession guard)
    {
        if (guard.WriteAttempted || guard.QueryFailed) throw new InvalidOperationException("A diagnostic query failed or attempted to write data.");
        if (result.Value == null || result.StatusCode >= 400) throw new InvalidOperationException("Invalid query response.");
        var payload = JToken.FromObject(result.Value);
        if (definition.AllowEmptyArray && payload is JArray { Count: 0 }) return new JArray();
        if (payload is not JObject obj || obj["data"] is not JArray rows || rows.Count > 2)
            throw new InvalidOperationException("Invalid list response shape.");
        foreach (var row in rows)
        {
            if (row is not JObject record || record[definition.Key] == null || record[definition.Key]!.Type == JTokenType.Null ||
                definition.Fields.Any(field => record.Property(field, StringComparison.Ordinal) == null))
                throw new InvalidOperationException("Required fields missing from query response.");
            if (definition.Key == "Id" && (!long.TryParse(record["Id"]!.ToString(), out var id) || id <= 0))
                throw new InvalidOperationException("Invalid record ID in query response.");
        }
        if (definition.Key == "Id" && rows.Select(row => row["Id"]!.ToString()).Distinct().Count() != rows.Count)
            throw new InvalidOperationException("Duplicate record IDs in query response.");
        return rows;
    }

    private static bool IsOrdered(JArray rows, string key, bool descending)
    {
        if (rows.Count < 2) return true;
        if (rows[0][key] is not JValue first || rows[1][key] is not JValue second) return false;
        return descending ? first.CompareTo(second) >= 0 : first.CompareTo(second) <= 0;
    }

    private static BackofficeCheckOutcome Pass(string message) => new(BackofficeTestStatus.Passed, message);
    private static BackofficeCheckOutcome Fail(string message) => new(BackofficeTestStatus.Failed, message);

    private static Task<JsonResult> Query(string key, DataSourceLoadOptions options, IServiceProvider services) => key switch
    {
            "orders" => services.GetRequiredService<IOrderAppService>().GetAllListForDiagnostics(options),
            "contacts" => services.GetRequiredService<IContactAppService>().GetContactListAll(options),
            "products" => services.GetRequiredService<IProductAppService>().GetAllList(options, null, null, false),
            "stock-alerts" => services.GetRequiredService<IProductAppService>().SaleQuantityStaging(options),
            "spec-types" => services.GetRequiredService<ISpecificationAppService>().GetAllTypeList(options),
            "specs" => services.GetRequiredService<ISpecificationAppService>().GetAllSpecList(options),
            "certificates" => services.GetRequiredService<ITechnicalCertificateAppService>().GetAllList(options),
            "menus" => services.GetRequiredService<IWebMenuApplication>().GetAllList(options, null),
            "directories" => services.GetRequiredService<IDirectoryAppService>().GetAllList(options, false),
            "articles" => services.GetRequiredService<IArticleAppService>().GetAllList(options),
            "advertise-directories" => services.GetRequiredService<IDirectoryAppService>().GetAdvertiseList(options),
            "enter-ads" => services.GetRequiredService<IAdvertiseAppService>().GetList(options, 1),
            "side-ads" => services.GetRequiredService<IAdvertiseAppService>().GetList(options, 2),
            "members" => services.GetRequiredService<IMemberAppService>().GetAllFrontList(options),
            "member-roles" => services.GetRequiredService<IMemberAppService>().GetDevAllRole(options),
            "managers" => services.GetRequiredService<IMemberAppService>().GetAllManagerList(options),
            "groups" => services.GetRequiredService<IUserHabitsAppService>().GetUserGroupList(options),
            "freight" => services.GetRequiredService<IFreightAppService>().GetAllList(options),
            "boxes" => services.GetRequiredService<IFreightAppService>().GetLogisticsBoxAllList(options),
            "campaigns" => services.GetRequiredService<IMarketingAppService>().GetAllList(options),
            "recipients" => services.GetRequiredService<INewsletterAppService>().GetRecipients(options),
            "search" => services.GetRequiredService<ICustSearchAppService>().GetAll(options),
            "marquee-list" => services.GetRequiredService<IMarqueeAppService>().GetAllList(options),
            "tag-group-list" => services.GetRequiredService<ITagAppService>().GetAllGroupList(options),
            "audit-list" => services.GetRequiredService<IAuditLogAppService>().GetAllList(options),
            "remote-page" => services.GetRequiredService<IRemoteAppService>().GetAllList(options),
            "remote-site" => services.GetRequiredService<IRemoteAppService>().GetPageList(options),
            "flow-day" => services.GetRequiredService<IFlowSizeAppService>().GetFlowSizesList("day", options),
            "flow-month" => services.GetRequiredService<IFlowSizeAppService>().GetFlowSizesList("month", options),
            "flow-year" => services.GetRequiredService<IFlowSizeAppService>().GetFlowSizesList("year", options),
            "bonus-records" => services.GetRequiredService<IBonusManagementAppService>().GetBonusLogForDataGrid(options),
            "bonus-members" => services.GetRequiredService<IBonusManagementAppService>().GetFrontUsers(options, false),
        _ => throw new InvalidOperationException("Unknown diagnostic query.")
    };
}
