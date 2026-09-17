using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Domains;
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
    PlatformAuditor auditor) : ControllerBase
{
    private const int ListLimit = 1000;

    [HttpGet]
    public async Task<WebsiteListResult> GetList()
    {
        // 左外接：客戶被軟刪除時，網站仍要留在清單上，不能無聲消失。
        var rows = await (
            from site in db.PlatformWebsites.AsNoTracking()
            join customer in db.Companies.AsNoTracking()
                on site.FK_CompanyId equals customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            orderby site.Status,
                    site.ServiceEndDate == null,
                    site.ServiceEndDate
            select new
            {
                site.Id,
                site.Name,
                site.FK_CompanyId,
                CustomerName = customer == null ? null : customer.Name,
                CustomerTaxId = customer == null ? null : customer.TaxID,
                site.Level,
                site.Status,
                site.Url,
                site.ServiceStartDate,
                site.ServiceEndDate,
                DomainEndDate = site.Domain == null ? (DateTime?)null : site.Domain.EndDate
            })
            .Take(ListLimit + 1)   // 多拿一筆，用來判斷是否被截斷
            .ToListAsync(HttpContext.RequestAborted);

        // 用伺服器日期計算，與總覽頁「即將到期」一致
        var today = DateTime.Today;

        var items = rows
            .Take(ListLimit)
            .Select(row => new WebsiteListItemDto(
                row.Id,
                row.Name,
                row.FK_CompanyId,
                row.CustomerName,
                row.CustomerTaxId,
                row.Level,
                row.Level?.ToString() ?? string.Empty,
                row.Status,
                row.Status.ToString(),
                row.Url,
                row.ServiceStartDate,
                row.ServiceEndDate,
                row.ServiceEndDate is null ? null : (row.ServiceEndDate.Value.Date - today).Days,
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
        var domainId = await ValidateRequestAsync(request);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var site = new PlatformWebsite();
        Apply(request, domainId, site);

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

        var domainId = await ValidateRequestAsync(request);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        Apply(request, domainId, site);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);

        var detail = await LoadDetailAsync(site.Id);
        if (detail is null)
            return NotFound();

        return detail;
    }

    private Task<WebsiteDetailDto?> LoadDetailAsync(long id)
    {
        return (
            from site in db.PlatformWebsites.AsNoTracking()
            join customer in db.Companies.AsNoTracking()
                on site.FK_CompanyId equals customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            where site.Id == id
            select new WebsiteDetailDto
            {
                Id = site.Id,
                FK_CompanyId = site.FK_CompanyId,
                Name = site.Name,
                Level = site.Level,
                HostLocation = site.HostLocation,
                ServiceStartDate = site.ServiceStartDate,
                ServiceEndDate = site.ServiceEndDate,
                Status = site.Status,
                TerminatedDate = site.TerminatedDate,
                IsDomainPending = site.IsDomainPending,
                Url = site.Url,
                Domain = site.Domain == null
                    ? null
                    : new DomainSummaryDto(
                        site.Domain.Id,
                        site.Domain.DomainName,
                        site.Domain.Registrar,
                        site.Domain.EndDate),
                Remark = site.Remark,
                Customer = customer == null
                    ? null
                    : new WebsiteCustomerDto(
                        customer.Id,
                        customer.Name,
                        customer.TaxID,
                        customer.Phone,
                        customer.Email,
                        customer.Contact)
            })
            .FirstOrDefaultAsync(HttpContext.RequestAborted);
    }

    /// <summary>回傳依網址比對出的網域 Id（待申請或未填網址時為 null）。</summary>
    private async Task<long?> ValidateRequestAsync(WebsiteSaveRequest request)
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

        if (request.FK_CompanyId > 0 &&
            !await db.Companies.AnyAsync(
                company => company.Id == request.FK_CompanyId,
                HttpContext.RequestAborted))
            ModelState.AddModelError(nameof(request.FK_CompanyId), "找不到對應的客戶資料，請重新帶出客戶。");

        var hasUrl = !string.IsNullOrWhiteSpace(request.Url);
        if (request.IsDomainPending)
        {
            if (hasUrl)
                ModelState.AddModelError(nameof(request.Url), "網域待申請時不可填寫網址。");
            return null;
        }

        if (!hasUrl)
            return null;

        // 網址 → 網域由後端比對，不相信前端
        var host = PlatformDomainName.ToHost(request.Url);
        if (host is null)
        {
            ModelState.AddModelError(nameof(request.Url), "網址格式不正確，例：https://www.example.com.tw");
            return null;
        }

        var domain = await db.PlatformDomains
            .AsNoTracking()
            .FindBestMatchAsync(host, HttpContext.RequestAborted);
        if (domain is null)
            ModelState.AddModelError(nameof(request.Url), $"網域管理中沒有「{host}」對應的網域，請先建立網域資料。");

        return domain?.Id;
    }

    private static void Apply(WebsiteSaveRequest request, long? domainId, PlatformWebsite site)
    {
        site.FK_CompanyId = request.FK_CompanyId;
        site.Name = request.Name.Trim();
        site.Level = request.Level;
        site.HostLocation = Clean(request.HostLocation);
        site.ServiceStartDate = request.ServiceStartDate?.Date;
        site.ServiceEndDate = request.ServiceEndDate?.Date;
        site.Status = request.Status;
        site.TerminatedDate = request.TerminatedDate?.Date;
        site.IsDomainPending = request.IsDomainPending;
        site.Url = request.IsDomainPending ? null : Clean(request.Url);
        site.FK_PlatformDomainId = domainId;
        site.Remark = Clean(request.Remark);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}