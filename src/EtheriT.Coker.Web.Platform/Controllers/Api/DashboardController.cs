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

        // 已註銷的網站不算「管理中」。
        // 已綁定站台時名稱與到期日以後台 Website 為準，未綁定才用合約上的預定值；
        // FK_WebsiteId 有唯一索引，左外接不會讓筆數變多。
        var activeWebsites =
            from contract in db.PlatformWebsites.AsNoTracking()
            where contract.Status != PlatformWebsiteStatusEnum.註銷
            join linked in db.Websites.AsNoTracking().Where(item => !item.IsDeleted)
                on contract.FK_WebsiteId equals (long?)linked.Id into linkedSites
            from linked in linkedSites.DefaultIfEmpty()
            select new
            {
                contract.Id,
                contract.FK_CompanyId,
                Name = linked != null ? linked.Title : contract.Name,
                EndDate = linked != null ? linked.EndDate : contract.ServiceEndDate
            };

        var websiteCount = await activeWebsites.CountAsync(cancellationToken);

        // 只算今天到 30 天內；已過期的不在「即將到期」
        var expiring = activeWebsites.Where(site =>
            site.EndDate != null &&
            site.EndDate >= today &&
            site.EndDate <= deadline);

        var expiringCount = await expiring.CountAsync(cancellationToken);

        // 左外接：客戶被軟刪除時網站仍要列出，前端顯示「（客戶已刪除）」
        var expiringWebsites = await (
            from site in expiring
            join customer in db.Companies.AsNoTracking()
                on site.FK_CompanyId equals customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            orderby site.EndDate, site.Id
            select new ExpiringWebsiteDto(
                site.Id,
                site.Name,
                customer == null ? null : customer.Name,
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