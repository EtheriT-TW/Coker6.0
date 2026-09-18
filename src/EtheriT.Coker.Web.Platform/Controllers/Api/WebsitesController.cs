using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Domains;
using EtheriT.Coker.Web.Platform.Models.Websites;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

// 不提供 DELETE：網站以「註銷」取代刪除。
[ApiController]
[Route("api/websites")]
public sealed class WebsitesController(
    CokerDbContext db,
    PlatformAuditor auditor) : ControllerBase
{
    private const int ListLimit = 1000;
    private const int SiteOptionLimit = 20;

    /// <summary>未被軟刪除的實際站台。沒有全域過濾器，每個查詢都得經過這裡。</summary>
    private IQueryable<Website> ActiveSites =>
        db.Websites.AsNoTracking().Where(site => !site.IsDeleted);

    /// <summary>未被軟刪除的合約資料（Platform 自己維護的那張表）。</summary>
    private IQueryable<PlatformWebsite> ActiveContracts =>
        db.PlatformWebsites.AsNoTracking().Where(contract => !contract.IsDeleted);

    /// <summary>站台 → 選項 DTO 的共用投影，讓搜尋與單筆查詢的欄位不會走鐘。</summary>
    private static readonly Expression<Func<Website, WebsiteSiteOptionDto>> ToSiteOption =
        site => new WebsiteSiteOptionDto(
            site.Id,
            site.OrgName,
            site.Title,
            site.DefaultUrl,
            site.Level,
            site.Locale,
            site.StartDate,
            site.EndDate);

    /// <summary>
    /// 網站管理清單＝「後台站台」∪「僅合約」。
    /// 分兩段查再於記憶體合併：兩邊欄位形狀不同，硬湊成一個 EF UNION 反而難讀難維護。
    /// </summary>
    [HttpGet]
    public async Task<WebsiteListResult> GetList()
    {
        // ① 後台所有站台，左外接掛上合約（沒建合約的，合約欄位一律為 null）
        var siteRows = await (
            from site in ActiveSites
            join contract in ActiveContracts
                on (long?)site.Id equals contract.FK_WebsiteId into contractGroup
            from contract in contractGroup.DefaultIfEmpty()
                // 客戶被軟刪除時，網站仍要留在清單上，不能無聲消失
            join customer in db.Companies.AsNoTracking()
                on contract.FK_CompanyId equals customer.Id into customerGroup
            from customer in customerGroup.DefaultIfEmpty()
            select new
            {
                WebsiteId = (long?)site.Id,
                site.OrgName,
                SiteTitle = site.Title,
                SiteUrl = site.DefaultUrl,
                SiteLevel = (WebsiteLevelEnum?)site.Level,
                PlatformWebsiteId = contract == null ? (long?)null : contract.Id,
                ContractName = contract == null ? null : contract.Name,
                ContractLevel = contract == null ? null : contract.Level,
                ContractUrl = contract == null ? null : contract.Url,
                FK_CompanyId = contract == null ? (long?)null : contract.FK_CompanyId,
                CustomerName = customer == null ? null : customer.Name,
                CustomerTaxId = customer == null ? null : customer.TaxID,
                Status = contract == null ? (PlatformWebsiteStatusEnum?)null : contract.Status,
                ServiceStartDate = contract == null ? null : contract.ServiceStartDate,
                ServiceEndDate = contract == null ? null : contract.ServiceEndDate,
                DomainEndDate = contract == null || contract.Domain == null
                    ? (DateTime?)null
                    : contract.Domain.EndDate
            })
            .Take(ListLimit + 1)
            .ToListAsync(HttpContext.RequestAborted);

        // ② 合約已建、但還沒對應到「有效」站台。
        //    NOT EXISTS 不可省略：綁定的站台被軟刪除時，①②都撈不到，那筆合約會憑空消失。
        var contractRows = await (
            from contract in ActiveContracts
            where contract.FK_WebsiteId == null
                  || !ActiveSites.Any(site => site.Id == contract.FK_WebsiteId)
            join customer in db.Companies.AsNoTracking()
                on contract.FK_CompanyId equals customer.Id into customerGroup
            from customer in customerGroup.DefaultIfEmpty()
            select new
            {
                contract.Id,
                contract.Name,
                contract.Level,
                contract.Url,
                contract.FK_CompanyId,
                CustomerName = customer == null ? null : customer.Name,
                CustomerTaxId = customer == null ? null : customer.TaxID,
                contract.Status,
                contract.ServiceStartDate,
                contract.ServiceEndDate,
                DomainEndDate = contract.Domain == null ? (DateTime?)null : contract.Domain.EndDate
            })
            .Take(ListLimit + 1)
            .ToListAsync(HttpContext.RequestAborted);

        // 用伺服器日期計算，與總覽頁「即將到期」一致
        var today = DateTime.Today;

        var items = siteRows
            .Select(row => new WebsiteListItemDto(
                RowKey: $"W{row.WebsiteId}",
                PlatformWebsiteId: row.PlatformWebsiteId,
                WebsiteId: row.WebsiteId,
                // 合約名稱優先；沒建合約就用站台自己的標題，標題空的再退回 OrgName
                Name: FirstNonBlank(row.ContractName, row.SiteTitle, row.OrgName),
                OrgName: row.OrgName,
                FK_CompanyId: row.FK_CompanyId,
                CustomerName: row.CustomerName,
                CustomerTaxId: row.CustomerTaxId,
                // 版本以站台實際設定為準，合約可能填錯
                Level: row.SiteLevel,
                LevelText: row.SiteLevel?.ToString() ?? string.Empty,
                Status: row.Status,
                StatusText: row.Status?.ToString() ?? string.Empty,
                Url: FirstNonBlank(row.SiteUrl, row.ContractUrl),
                ServiceStartDate: row.ServiceStartDate,
                ServiceEndDate: row.ServiceEndDate,
                RemainingDays: ToRemainingDays(row.ServiceEndDate, today),
                DomainEndDate: row.DomainEndDate,
                IsPending: row.PlatformWebsiteId is null))
            .Concat(contractRows
                .Select(row => new WebsiteListItemDto(
                    RowKey: $"P{row.Id}",
                    PlatformWebsiteId: row.Id,
                    WebsiteId: null,
                    Name: row.Name,
                    OrgName: null,
                    FK_CompanyId: row.FK_CompanyId,
                    CustomerName: row.CustomerName,
                    CustomerTaxId: row.CustomerTaxId,
                    Level: row.Level,
                    LevelText: row.Level?.ToString() ?? string.Empty,
                    Status: row.Status,
                    StatusText: row.Status.ToString(),
                    Url: row.Url,
                    ServiceStartDate: row.ServiceStartDate,
                    ServiceEndDate: row.ServiceEndDate,
                    RemainingDays: ToRemainingDays(row.ServiceEndDate, today),
                    DomainEndDate: row.DomainEndDate,
                    IsPending: false)))
            // Status 為 null（還沒建合約）排最前，其餘依狀態、到期日
            .OrderBy(item => item.Status)
            .ThenBy(item => item.ServiceEndDate is null)
            .ThenBy(item => item.ServiceEndDate)
            .ToList();

        return new WebsiteListResult(
            items.Take(ListLimit).ToList(),
            items.Count > ListLimit);
    }

    private static int? ToRemainingDays(DateTime? endDate, DateTime today) =>
        endDate is null ? null : (endDate.Value.Date - today).Days;

    private static string FirstNonBlank(params string?[] values) =>
        Array.Find(values, value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

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
        var domainId = await ValidateRequestAsync(request, 0);
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

    /// <summary>
    /// 綁定用：以 OrgName／站名／網址搜尋實際站台。
    /// 本專案沒有 IsDeleted 全域查詢過濾器，軟刪除一律要自己過濾。
    /// </summary>
    [HttpGet("site-options")]
    public async Task<WebsiteSiteOptionResult> GetSiteOptions([FromQuery] string? keyword)
    {
        var text = keyword?.Trim() ?? string.Empty;
        var query = ActiveSites;

        if (text.Length > 0)
            query = query.Where(site =>
                site.OrgName.Contains(text) ||
                site.Title.Contains(text) ||
                (site.DefaultUrl != null && site.DefaultUrl.Contains(text)));

        var rows = await query
            .OrderBy(site => site.OrgName)
            .Select(ToSiteOption)
            .Take(SiteOptionLimit + 1)      // 多拿一筆判斷截斷，與 GetList 一致
            .ToListAsync(HttpContext.RequestAborted);

        return new WebsiteSiteOptionResult(
            rows.Take(SiteOptionLimit).ToList(),
            rows.Count > SiteOptionLimit);
    }

    /// <summary>依 Id 取單筆站台。清單上尚未建合約的列會直接指定站台，不能只靠關鍵字搜尋碰運氣。</summary>
    [HttpGet("site-options/{id:long}")]
    public async Task<ActionResult<WebsiteSiteOptionDto>> GetSiteOption(long id)
    {
        var site = await ActiveSites
            .Where(item => item.Id == id)
            .Select(ToSiteOption)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        return site is null ? NotFound() : site;
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<WebsiteDetailDto>> Update(long id, WebsiteSaveRequest request)
    {
        var site = await db.PlatformWebsites
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);
        if (site is null)
            return NotFound();

        var domainId = await ValidateRequestAsync(request, site.Id);
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
            // 站台可能還沒綁定或已被刪除，一律左外接
            join linked in ActiveSites
                on site.FK_WebsiteId equals (long?)linked.Id into linkedSites
            from linked in linkedSites.DefaultIfEmpty()
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
                FK_WebsiteId = site.FK_WebsiteId,
                LinkedSite = linked == null
                    ? null
                    : new WebsiteSiteOptionDto(
                        linked.Id,
                        linked.OrgName,
                        linked.Title,
                        linked.DefaultUrl,
                        linked.Level,
                        linked.Locale,
                        linked.StartDate,
                        linked.EndDate),
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
    /// <param name="currentId">編輯時為自己的 Id，新增時傳 0；用來排除「綁定被自己佔用」的誤判。</param>
    private async Task<long?> ValidateRequestAsync(WebsiteSaveRequest request, long currentId)
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

        await ValidateLinkedSiteAsync(request, currentId);

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

    /// <summary>
    /// 驗證「對應站台」：站台要存在且未被軟刪除，且一個站台只能被一筆網站資料綁定。
    /// </summary>
    private async Task ValidateLinkedSiteAsync(WebsiteSaveRequest request, long currentId)
    {
        if (request.FK_WebsiteId is not long siteId)
            return;

        if (siteId <= 0)
        {
            ModelState.AddModelError(nameof(request.FK_WebsiteId), "對應站台不正確。");
            return;
        }

        if (!await ActiveSites.AnyAsync(site => site.Id == siteId, HttpContext.RequestAborted))
        {
            ModelState.AddModelError(nameof(request.FK_WebsiteId), "找不到對應的站台，請重新搜尋。");
            return;
        }

        // currentId 為自己的 Id（新增時 0），排除「被自己佔用」的誤判
        var takenBy = await db.PlatformWebsites
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.FK_WebsiteId == siteId && item.Id != currentId)
            .Select(item => item.Name)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (takenBy is not null)
            ModelState.AddModelError(
                nameof(request.FK_WebsiteId),
                $"這個站台已經被「{takenBy}」綁定，請先解除該筆的綁定。");
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
        site.FK_WebsiteId = request.FK_WebsiteId;   // 有效性已在 ValidateRequestAsync 擋掉
        site.Remark = Clean(request.Remark);
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}