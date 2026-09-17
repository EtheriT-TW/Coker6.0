namespace EtheriT.Coker.Web.Platform.Models.Dashboard;

public sealed record ExpiringWebsiteDto(
    long Id,
    string Name,
    string? CustomerName,
    DateTime? ServiceEndDate);

public sealed record DashboardSummaryDto(
    int CustomerCount,
    int WebsiteCount,
    int ExpiringCount,
    int ExpiringWithinDays,
    IReadOnlyList<ExpiringWebsiteDto> ExpiringWebsites);