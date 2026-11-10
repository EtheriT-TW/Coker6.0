using EtheriT.Coker.Application;
using EtheriT.Coker.Application.Shared.Dashboard;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.Application.Shared.ThirdParty;
using EtheriT.Coker.Application.StoreSet;
using Newtonsoft.Json.Linq;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static class BackofficeObjectChecks
{
    private static readonly IReadOnlyDictionary<string, string> Pages = new Dictionary<string, string>
    {
        ["object/website"] = "SystemManagement.Index",
        ["object/seo"] = "SystemManagement.SEO",
        ["object/shop-settings"] = "StoreSettings.Index",
        ["object/smtp-settings"] = "StoreSettings.SMTPSettings",
        ["object/member-settings"] = "MemberManagement.Index",
        ["object/payment-settings"] = "StoreSettings.PaymentSettings",
        ["object/logistics-settings"] = "StoreSettings.LogisticsSettings",
        ["object/components"] = "Page.ComponerManager",
        ["object/menu-tree"] = "Page.Index",
        ["object/traffic"] = "Dashboard.Index",
        ["object/traffic-heatmap"] = "Dashboard.Index",
        ["object/popular-pages"] = "Dashboard.Index",
        ["object/contact-summary"] = "Dashboard.Index",
        ["object/order-trend"] = "Dashboard.Index",
        ["object/revenue-day"] = "OrderManagement.RevenueReport",
        ["object/revenue-month"] = "OrderManagement.RevenueReport"
    };

    public static bool Supports(string page, string? executor) =>
        executor != null && Pages.TryGetValue(executor, out var expected) && expected == page;

    public static async Task<BackofficeCheckOutcome?> ExecuteAsync(string page, string? executor,
        IServiceProvider services, CancellationToken token)
    {
        if (executor == null || !Pages.TryGetValue(executor, out var expected) || expected != page) return null;
        using var guard = new DiagnosticsReadOnlySession(token);
        token.ThrowIfCancellationRequested();
        var passed = false;
        if (executor.StartsWith("object/revenue-", StringComparison.Ordinal))
        {
            var granularity = executor.EndsWith("day", StringComparison.Ordinal) ? RevenueReportGranularity.Day : RevenueReportGranularity.Month;
            var report = await services.GetRequiredService<IRevenueReportAppService>().GetRevenue(granularity, DateTime.Today.AddDays(-7), DateTime.Today);
            passed = report.Granularity == granularity && report.StartDate <= report.EndDate &&
                report.Items != null && report.Items.All(item => item.OrderCount >= 0 && item.InvalidOrderCount >= 0 &&
                    item.PeriodStart >= report.StartDate.AddMonths(-1) && item.PeriodStart <= report.EndDate &&
                    (item.OrderCount != 0 || item.AverageOrderValue == 0));
        }
        else if (executor is "object/seo" or "object/shop-settings" or "object/smtp-settings" or "object/member-settings")
        {
            var groups = executor switch
            {
                "object/seo" => new List<long> { 1, 8 }, "object/shop-settings" => new List<long> { 2 },
                "object/smtp-settings" => new List<long> { 3 }, _ => new List<long> { 7 }
            };
            var value = await services.GetRequiredService<IStoreSetAppService>().getAll(groups);
            passed = value.Success && value.storeGroups != null && value.storeGroups.Count > 0;
        }
        else if (executor is "object/payment-settings" or "object/logistics-settings")
        {
            var value = await services.GetRequiredService<IThirdPartyAppService>().GetAllThirdParty(
                executor == "object/payment-settings" ? ThirdPartyServiceTypeEnum.Payment : ThirdPartyServiceTypeEnum.Logistics);
            passed = value.Success && value.Object != null;
        }
        else if (executor == "object/website")
        {
            passed = (await services.GetRequiredService<IWebsiteApplication>().GetWebsiteData()).Success;
        }
        else if (executor == "object/components")
        {
            var value = await services.GetRequiredService<IObjectTypeAppService>().GetAll();
            passed = value.Success;
        }
        else if (executor == "object/menu-tree")
        {
            var value = await services.GetRequiredService<IWebMenuApplication>().GetAll();
            passed = value.Success && value.Maps != null;
        }
        else
        {
            var service = services.GetRequiredService<IDashboardAppService>();
            switch (executor)
            {
                case "object/traffic":
                    var traffic = await service.GetTraffic(7);
                    passed = traffic.Items != null && traffic.Items.All(row => row.PageViews >= 0 && row.Visitors >= 0); break;
                case "object/traffic-heatmap":
                    var heatmap = await service.GetTrafficHeatmap(7);
                    passed = heatmap.Items != null && heatmap.Items.Count == 7 * 24
                        && heatmap.Items.Select(row => (row.DayOfWeek, row.Hour)).Distinct().Count() == 7 * 24
                        && heatmap.Items.All(row => row.DayOfWeek is >= 1 and <= 7 && row.Hour is >= 0 and <= 23 && row.TotalViews >= 0 && row.TotalVisitors >= 0); break;
                case "object/popular-pages":
                    var popular = await service.GetPopularPages(5, DateTime.Today.AddDays(-7), DateTime.Today);
                    passed = popular != null && popular.Count <= 5 && popular.All(row => row.Views >= 0 && row.Visitors >= 0); break;
                case "object/contact-summary":
                    var contacts = await service.GetContacts(5);
                    passed = contacts.PendingCount >= 0 && contacts.Recent != null && contacts.Recent.Count <= 5; break;
                case "object/order-trend":
                    var orders = await service.GetOrderTrend(7);
                    passed = orders.Items != null && orders.Items.All(row => row.OrderCount >= 0); break;
            }
        }
        if (guard.WriteAttempted || guard.QueryFailed) passed = false;
        return new(passed ? BackofficeTestStatus.Passed : BackofficeTestStatus.Failed,
            passed ? "實際查詢與資料檢查通過。" : "查詢或資料檢查未通過，請聯絡系統維護人員。");
    }
}
