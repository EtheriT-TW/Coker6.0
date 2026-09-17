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

        // 已註銷的網站不算「管理中」
        var activeWebsites = db.PlatformWebsites
            .AsNoTracking()
            .Where(site => site.Status != PlatformWebsiteStatusEnum.註銷);

        var websiteCount = await activeWebsites.CountAsync(cancellationToken);

        // 只算今天到 30 天內；已過期的不在「即將到期」
        var expiring = activeWebsites.Where(site =>
            site.ServiceEndDate != null &&
            site.ServiceEndDate >= today &&
            site.ServiceEndDate <= deadline);

        var expiringCount = await expiring.CountAsync(cancellationToken);

        // 左外接：客戶被軟刪除時網站仍要列出，前端顯示「（客戶已刪除）」
        var expiringWebsites = await (
            from site in expiring
            join customer in db.Companies.AsNoTracking()
                on site.FK_CompanyId equals customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            orderby site.ServiceEndDate, site.Id
            select new ExpiringWebsiteDto(
                site.Id,
                site.Name,
                customer == null ? null : customer.Name,
                site.ServiceEndDate))
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