using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(CokerDbContext db) : ControllerBase
{
    private const int ExpiringWithinDays = 30;
    private const int ExpiringListLimit = 10;

    [HttpGet("summary")]
    public async Task<DashboardSummaryDto> GetSummary()
    {
        var cancellationToken = HttpContext.RequestAborted;
        var today = DateTime.Today;
        var deadline = today.AddDays(ExpiringWithinDays);

        // DbContext 不可並行查詢，逐一 await
        var customerCount = await db.Companies.CountAsync(cancellationToken);

        // 已註銷的網站不算「管理中」。一筆＝一個後台站台（含尚未在 Platform 編輯過的，狀態預設正常）。
        var activeWebsites =
            from site in db.Websites.AsNoTracking()
            where !site.IsDeleted && site.ServiceStatus != PlatformWebsiteStatusEnum.註銷
            select new
            {
                site.Id,
                Name = site.Title,
                site.EndDate,
                // 後台綁定的公司，多筆時取最早那筆；沒綁公司時為 null
                CustomerName = site.Company
                    .Where(mapping => !mapping.IsDeleted && mapping.Company != null)
                    .OrderBy(mapping => mapping.Id)
                    .Select(mapping => mapping.Company!.Name)
                    .FirstOrDefault()
            };

        var websiteCount = await activeWebsites.CountAsync(cancellationToken);

        // 只算今天到 30 天內；已過期的不在「即將到期」
        var expiring = activeWebsites.Where(site =>
            site.EndDate != null &&
            site.EndDate >= today &&
            site.EndDate <= deadline);

        var expiringCount = await expiring.CountAsync(cancellationToken);

        var expiringWebsites = await expiring
            .OrderBy(site => site.EndDate)
            .ThenBy(site => site.Id)
            .Select(site => new ExpiringWebsiteDto(
                site.Id,
                site.Name,
                site.CustomerName,
                site.EndDate))
            .Take(ExpiringListLimit)
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto(
            customerCount,
            websiteCount,
            expiringCount,
            ExpiringWithinDays,
            expiringWebsites);
    }
}