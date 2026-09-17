using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Websites;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

// 不提供 DELETE：網站以「註銷」取代刪除。
[ApiController]
[Route("api/websites")]
public sealed class WebsitesController(
    CokerDbContext db,
    PlatformAuditor auditor,
    PlatformDomainPasswordProtector domainPasswordProtector) : ControllerBase
{
    private const int ListLimit = 1000;

    [HttpGet]
    public async Task<WebsiteListResult> GetList()
    {
        // 左外接：客戶被軟刪除時，網站仍要留在清單上，不能無聲消失。
        var rows = await (
            from site in db.PlatformWebsites.AsNoTracking()
            join customer in db.PlatformCustomers.AsNoTracking()
                on site.FK_PlatformCustomerId equals customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            orderby site.Status,
                    site.ServiceEndDate == null,
                    site.ServiceEndDate
            select new
            {
                site.Id,
                site.Name,
                site.FK_PlatformCustomerId,
                CustomerName = customer == null ? null : customer.Name,
                CustomerTaxId = customer == null ? null : customer.TaxId,
                site.Level,
                site.Status,
                site.DomainName,
                site.ServiceEndDate,
                site.DomainEndDate
            })
            .Take(ListLimit + 1)   // 多拿一筆，用來判斷是否被截斷
            .ToListAsync(HttpContext.RequestAborted);

        var items = rows
            .Take(ListLimit)
            .Select(row => new WebsiteListItemDto(
                row.Id,
                row.Name,
                row.FK_PlatformCustomerId,
                row.CustomerName,
                row.CustomerTaxId,
                row.Level,
                row.Level?.ToString() ?? string.Empty,
                row.Status,
                row.Status.ToString(),
                row.DomainName,
                row.ServiceEndDate,
                row.DomainEndDate))
            .ToList();

        return new WebsiteListResult(items, rows.Count > ListLimit);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<WebsiteDetailDto>> GetDetail(long id)
    {
        var detail = await LoadDetailAsync(id);
        if (detail is null)
            return NotFound();

        return detail;
    }

    [HttpPost]
    public async Task<ActionResult<WebsiteDetailDto>> Create(WebsiteSaveRequest request)
    {
        await ValidateRequestAsync(request);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var site = new PlatformWebsite();
        Apply(request, site);

        db.PlatformWebsites.Add(site);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);

        var detail = await LoadDetailAsync(site.Id);
        if (detail is null)
            return NotFound();

        return detail;
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<WebsiteDetailDto>> Update(long id, WebsiteSaveRequest request)
    {
        var site = await db.PlatformWebsites
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);
        if (site is null)
            return NotFound();

        await ValidateRequestAsync(request);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        Apply(request, site);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);

        var detail = await LoadDetailAsync(site.Id);
        if (detail is null)
            return NotFound();

        return detail;
    }

    /// <summary>
    /// 「眼睛」按鈕專用。目前是純讀取（暫不寫稽核），所以用 GET。
    /// ⚠ 日後若要補「誰看過」的稽核寫入，必須改成 POST 走防偽 token。
    /// </summary>
    [HttpGet("{id:long}/domain-password")]
    public async Task<ActionResult<DomainPasswordDto>> GetDomainPassword(long id)
    {
        var site = await db.PlatformWebsites
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new { item.DomainPasswordCipher })
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (site is null)
            return NotFound();

        // 解不開也回 200 + State=Unreadable，前端顯示提示而不是整頁失敗
        var result = domainPasswordProtector.Unprotect(site.DomainPasswordCipher);
        return new DomainPasswordDto(result.State, result.Password);
    }

    private Task<WebsiteDetailDto?> LoadDetailAsync(long id)
    {
        return (
            from site in db.PlatformWebsites.AsNoTracking()
            join customer in db.PlatformCustomers.AsNoTracking()
                on site.FK_PlatformCustomerId equals customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            where site.Id == id
            select new WebsiteDetailDto
            {
                Id = site.Id,
                FK_PlatformCustomerId = site.FK_PlatformCustomerId,
                Name = site.Name,
                Level = site.Level,
                HostLocation = site.HostLocation,
                ServiceStartDate = site.ServiceStartDate,
                ServiceEndDate = site.ServiceEndDate,
                Status = site.Status,
                TerminatedDate = site.TerminatedDate,
                IsDomainPending = site.IsDomainPending,
                DomainName = site.DomainName,
                DomainRegistrar = site.DomainRegistrar,
                DomainStartDate = site.DomainStartDate,
                DomainEndDate = site.DomainEndDate,
                HasDomainPassword = site.DomainPasswordCipher != null,
                Remark = site.Remark,
                Customer = customer == null
                    ? null
                    : new WebsiteCustomerDto(
                        customer.Id,
                        customer.Name,
                        customer.TaxId,
                        customer.Phone,
                        customer.Email,
                        customer.PrimaryContactName)
            })
            .FirstOrDefaultAsync(HttpContext.RequestAborted);
    }

    private async Task ValidateRequestAsync(WebsiteSaveRequest request)
    {
        // ASP.NET 預設不驗 enum 範圍，Status: 99 會原樣寫進資料庫
        if (!Enum.IsDefined(request.Status))
            ModelState.AddModelError(nameof(request.Status), "網站狀態不正確。");

        if (request.Level.HasValue && !Enum.IsDefined(request.Level.Value))
            ModelState.AddModelError(nameof(request.Level), "網站版本不正確。");

        // 註銷才准填註銷日期，且註銷時必填；前端 disabled 只是體驗，這裡才是真的擋
        var isTerminated = request.Status == PlatformWebsiteStatusEnum.註銷;
        if (isTerminated && request.TerminatedDate is null)
            ModelState.AddModelError(nameof(request.TerminatedDate), "網站狀態為「註銷」時，必須填寫註銷日期。");
        else if (!isTerminated && request.TerminatedDate is not null)
            ModelState.AddModelError(nameof(request.TerminatedDate), "只有網站狀態為「註銷」時才能填寫註銷日期。");

        if (request.ServiceStartDate is not null && request.ServiceEndDate is not null &&
            request.ServiceEndDate < request.ServiceStartDate)
            ModelState.AddModelError(nameof(request.ServiceEndDate), "網站到期日期不可早於開通日期。");

        if (request.DomainStartDate is not null && request.DomainEndDate is not null &&
            request.DomainEndDate < request.DomainStartDate)
            ModelState.AddModelError(nameof(request.DomainEndDate), "網域到期日期不可早於起始日期。");

        if (request.ClearDomainPassword && !string.IsNullOrWhiteSpace(request.DomainPassword))
            ModelState.AddModelError(nameof(request.DomainPassword), "已勾選清除網域密碼，不可同時輸入新密碼。");

        if (request.FK_PlatformCustomerId > 0 &&
            !await db.PlatformCustomers.AnyAsync(
                customer => customer.Id == request.FK_PlatformCustomerId,
                HttpContext.RequestAborted))
            ModelState.AddModelError(nameof(request.FK_PlatformCustomerId), "找不到對應的客戶資料，請重新以統一編號查詢。");
    }

    private void Apply(WebsiteSaveRequest request, PlatformWebsite site)
    {
        site.FK_PlatformCustomerId = request.FK_PlatformCustomerId;
        site.Name = request.Name.Trim();
        site.Level = request.Level;
        site.HostLocation = Clean(request.HostLocation);
        site.ServiceStartDate = request.ServiceStartDate?.Date;
        site.ServiceEndDate = request.ServiceEndDate?.Date;
        site.Status = request.Status;
        site.TerminatedDate = request.TerminatedDate?.Date;
        site.IsDomainPending = request.IsDomainPending;
        site.DomainName = Clean(request.DomainName);
        site.DomainRegistrar = Clean(request.DomainRegistrar);
        site.DomainStartDate = request.DomainStartDate?.Date;
        site.DomainEndDate = request.DomainEndDate?.Date;
        site.Remark = Clean(request.Remark);

        if (request.ClearDomainPassword)
            site.DomainPasswordCipher = null;
        else if (!string.IsNullOrWhiteSpace(request.DomainPassword))
            site.DomainPasswordCipher = domainPasswordProtector.Protect(request.DomainPassword);
        // 兩者皆無 → 保留原密文
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}