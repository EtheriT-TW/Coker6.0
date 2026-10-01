using System.Data;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Domains;
using EtheriT.Coker.Web.Platform.Models.Websites;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

// 網站資料直接讀寫後台 Websites；PlatformWebsites 已停用，僅保留作為搬移備份。
// 新增只建立 Website 與客戶綁定；目錄、商店設定、權限仍由後台人工補齊。
// 不提供 DELETE：網站以「註銷」取代刪除。
[ApiController]
[Route("api/websites")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = EtheriT.Coker.Authentication.Backoffice.BackofficeAuthorizationPolicies.PlatformDataManagement)]
public sealed class WebsitesController(
    CokerDbContext db,
    PlatformAuditor auditor) : ControllerBase
{
    private const int ListLimit = 1000;

    /// <summary>與種子資料相同；目前沒有任何程式依 Website.Type 分流。</summary>
    private const string DefaultWebsiteType = "website";

    /// <summary>
    /// OrgName 會當 upload 目錄名稱，也會直接嵌進後台寄信的正規表達式（MailAppService），
    /// 只允許小寫英數、- 與 _，且以英數開頭。
    /// </summary>
    private static readonly Regex OrgNamePattern = new("^[a-z0-9][a-z0-9_-]{1,99}$", RegexOptions.Compiled);

    /// <summary>未被軟刪除的實際站台。沒有全域過濾器，每個查詢都得經過這裡。</summary>
    private IQueryable<Website> ActiveSites =>
        db.Websites.AsNoTracking().Where(site => !site.IsDeleted);

    [HttpGet]
    public async Task<WebsiteListResult> GetList()
    {
        var rows = await ActiveSites
            // 依狀態、到期日排序；沒有到期日的排在最後
            .OrderBy(site => site.ServiceStatus)
            .ThenBy(site => site.EndDate == null)
            .ThenBy(site => site.EndDate)
            .Select(site => new
            {
                site.Id,
                site.OrgName,
                site.Title,
                site.DefaultUrl,
                site.Level,
                site.ServiceStatus,
                site.StartDate,
                site.EndDate,
                DomainEndDate = site.PlatformDomain == null ? (DateTime?)null : site.PlatformDomain.EndDate,
                // 後台綁定的公司；舊資料若有多筆固定取最早那筆（存檔時會被 SyncCompanyMappingAsync 擋下）。
                // 客戶被軟刪除時仍帶出名稱，網站不能無聲失去客戶。
                Customer = site.Company
                    .Where(mapping => !mapping.IsDeleted && mapping.Company != null)
                    .OrderBy(mapping => mapping.Id)
                    .Select(mapping => new
                    {
                        mapping.Company!.Id,
                        mapping.Company.Name,
                        mapping.Company.TaxID
                    })
                    .FirstOrDefault()
            })
            .Take(ListLimit + 1)
            .ToListAsync(HttpContext.RequestAborted);

        // 用伺服器日期計算，與總覽頁「即將到期」一致
        var today = DateTime.Today;

        var items = rows
            .Take(ListLimit)
            .Select(row => new WebsiteListItemDto(
                Id: row.Id,
                // 標題空的（舊資料）才退回 OrgName
                Name: FirstNonBlank(row.Title, row.OrgName),
                OrgName: row.OrgName,
                FK_CompanyId: row.Customer?.Id,
                CustomerName: row.Customer?.Name,
                CustomerTaxId: row.Customer?.TaxID,
                Level: row.Level,
                LevelText: row.Level.ToString(),
                Status: row.ServiceStatus,
                StatusText: row.ServiceStatus.ToString(),
                Url: row.DefaultUrl,
                ServiceStartDate: row.StartDate,
                ServiceEndDate: row.EndDate,
                RemainingDays: ToRemainingDays(row.EndDate, today),
                DomainEndDate: row.DomainEndDate))
            .ToList();

        return new WebsiteListResult(items, rows.Count > ListLimit);
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
    public async Task<ActionResult<WebsiteDetailDto>> Create(WebsiteCreateRequest request)
    {
        var orgName = request.OrgName.Trim().ToLowerInvariant();
        if (!OrgNamePattern.IsMatch(orgName))
            ModelState.AddModelError(nameof(request.OrgName),
                "站台代碼只能使用小寫英文、數字、- 與 _，長度 2～100 字，且須以英文或數字開頭。");

        var domainId = await ValidateRequestAsync(request);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var cancellationToken = HttpContext.RequestAborted;
        var strategy = db.Database.CreateExecutionStrategy();
        var siteId = await strategy.ExecuteAsync(async () =>
        {
            // Serializable：OrgName 沒有唯一索引，靠交易擋住兩人同時建立同一個代碼
            await using var transaction = await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            // 已刪除的站台也算：舊站的 upload 目錄可能還在，重複使用會讀到別人的檔案
            if (await db.Websites.AnyAsync(site => site.OrgName == orgName, cancellationToken))
                return (long?)null;

            var site = new Website
            {
                OrgName = orgName,
                Type = DefaultWebsiteType
            };
            Apply(request, domainId, site);
            db.Websites.Add(site);
            db.MappingCompanyAndWebsites.Add(new MappingCompanyAndWebsites
            {
                FK_CompanyId = request.FK_CompanyId,
                Website = site
            });
            await auditor.SaveChangesAsync(cancellationToken);

            // Level 設了資料庫預設值「形象」，EF 會把 0（簡約）當成沒填而改用預設值，
            // 存完讀回來不一樣就再寫一次
            if (site.Level != request.Level!.Value)
            {
                site.Level = request.Level.Value;
                await auditor.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return (long?)site.Id;
        });

        if (siteId is null)
        {
            ModelState.AddModelError(nameof(request.OrgName), $"站台代碼「{orgName}」已被使用（含已刪除的站台）。");
            return ValidationProblem(ModelState);
        }

        var detail = await LoadDetailAsync(siteId.Value);
        if (detail is null)
            return NotFound();

        return detail;
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<WebsiteDetailDto>> Update(long id, WebsiteSaveRequest request)
    {
        var site = await db.Websites
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, HttpContext.RequestAborted);
        if (site is null)
            return NotFound();

        var domainId = await ValidateRequestAsync(request);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        Apply(request, domainId, site);

        if (!await SyncCompanyMappingAsync(site.Id, request.FK_CompanyId))
            return ValidationProblem(ModelState);

        await auditor.SaveChangesAsync(HttpContext.RequestAborted);

        var detail = await LoadDetailAsync(site.Id);
        if (detail is null)
            return NotFound();

        return detail;
    }

    private Task<WebsiteDetailDto?> LoadDetailAsync(long id)
    {
        return (
            from site in ActiveSites
            where site.Id == id
            // 後台綁定的公司；舊資料若有多筆固定取最早那筆（存檔時會被 SyncCompanyMappingAsync 擋下）
            let companyId = db.MappingCompanyAndWebsites
                .Where(mapping => !mapping.IsDeleted && mapping.FK_WebsiteId == site.Id)
                .OrderBy(mapping => mapping.Id)
                .Select(mapping => (long?)mapping.FK_CompanyId)
                .FirstOrDefault()
            join customer in db.Companies.AsNoTracking()
                on companyId equals (long?)customer.Id into customers
            from customer in customers.DefaultIfEmpty()
            select new WebsiteDetailDto
            {
                Id = site.Id,
                OrgName = site.OrgName,
                FK_CompanyId = companyId ?? 0,
                Name = site.Title,
                Level = site.Level,
                Locale = site.Locale,
                HostLocation = site.HostLocation,
                ServiceStartDate = site.StartDate,
                ServiceEndDate = site.EndDate,
                Status = site.ServiceStatus,
                TerminatedDate = site.TerminatedDate,
                IsDomainPending = site.IsDomainPending,
                Url = site.IsDomainPending ? null : site.DefaultUrl,
                Domain = site.PlatformDomain == null
                    ? null
                    : new DomainSummaryDto(
                        site.PlatformDomain.Id,
                        site.PlatformDomain.DomainName,
                        site.PlatformDomain.Registrar,
                        site.PlatformDomain.EndDate,
                        site.PlatformDomain.PasswordCipher != null),
                Remark = site.PlatformRemark,
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

    /// <summary>回傳依網址比對出的網域 Id（待申請時為 null）。</summary>
    private async Task<long?> ValidateRequestAsync(WebsiteSaveRequest request)
    {
        // ASP.NET 預設不驗 enum 範圍，Status: 99 會原樣寫進資料庫
        if (!Enum.IsDefined(request.Status))
            ModelState.AddModelError(nameof(request.Status), "網站狀態不正確。");

        // 必填由 [Required] 擋，這裡只驗值是否合法
        if (request.Level.HasValue && !Enum.IsDefined(request.Level.Value))
            ModelState.AddModelError(nameof(request.Level), "網站版本不正確。");

        if (!string.IsNullOrWhiteSpace(request.Locale) && WebsiteLocales.Normalize(request.Locale) is null)
            ModelState.AddModelError(nameof(request.Locale), "語系不正確。");

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

        if (request.IsDomainPending)
        {
            if (!string.IsNullOrWhiteSpace(request.Url))
                ModelState.AddModelError(nameof(request.Url), "網域待申請時不可填寫網址。");
            return null;
        }

        // 寫進 Website.DefaultUrl，後台會拿來組金流、物流回呼網址，所以只收根網址
        var root = ToSiteRoot(request.Url);
        if (root is null)
        {
            ModelState.AddModelError(nameof(request.Url),
                "請輸入網站根網址，例：https://www.example.com.tw（不可包含路徑）；網域還沒申請請勾選「網域待申請」。");
            return null;
        }
        if (root.Length > 255)
        {
            ModelState.AddModelError(nameof(request.Url), "網址不可超過 255 個字元。");
            return null;
        }

        // 網址 → 網域由後端比對，不相信前端
        var host = PlatformDomainName.ToHost(root);
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

    private static void Apply(WebsiteSaveRequest request, long? domainId, Website site)
    {
        site.Title = request.Name.Trim();
        site.Level = request.Level!.Value;                        // [Required] 已擋
        site.Locale = WebsiteLocales.Normalize(request.Locale)!;  // [Required] 與格式驗證已擋
        site.StartDate = request.ServiceStartDate?.Date;
        site.EndDate = request.ServiceEndDate?.Date;
        site.ServiceStatus = request.Status;
        site.TerminatedDate = request.TerminatedDate?.Date;
        site.HostLocation = Clean(request.HostLocation);
        site.IsDomainPending = request.IsDomainPending;
        // 待申請時不動 DefaultUrl：後台網址必填，金流／物流回呼也靠它組網址
        if (!request.IsDomainPending)
            site.DefaultUrl = ToSiteRoot(request.Url);
        site.FK_PlatformDomainId = domainId;
        site.PlatformRemark = Clean(request.Remark);
    }

    /// <summary>
    /// 後台用 MappingCompanyAndWebsites 表示站台屬於哪家公司（一站一公司），
    /// 後台公司資訊、前台 SEO、訂單通知信副本都讀它。
    /// 換公司時直接改既有那筆的 FK_CompanyId，不採「軟刪除舊的、再新增」：
    /// 訂單通知信的查詢沒有排除軟刪除，留下舊紀錄可能讓信寄到舊公司。
    /// </summary>
    private async Task<bool> SyncCompanyMappingAsync(long siteId, long companyId)
    {
        var mappings = await db.MappingCompanyAndWebsites
            .Where(item => !item.IsDeleted && item.FK_WebsiteId == siteId)
            .ToListAsync(HttpContext.RequestAborted);

        // 舊資料可能一站多公司，無法判斷要改哪一筆，交給系統管理者處理
        if (mappings.Count > 1)
        {
            ModelState.AddModelError(nameof(WebsiteSaveRequest.FK_CompanyId),
                "此站台在後台綁定了多筆公司資料，請由系統管理者確認後再儲存。");
            return false;
        }

        if (mappings.Count == 0)
        {
            db.MappingCompanyAndWebsites.Add(new MappingCompanyAndWebsites
            {
                FK_CompanyId = companyId,
                FK_WebsiteId = siteId
            });
            return true;
        }

        // 公司沒變時 EF 不會產生 UPDATE，也不會動到稽核欄位
        mappings[0].FK_CompanyId = companyId;
        return true;
    }

    /// <summary>
    /// 後台 DefaultUrl 會被拿來組金流、物流回呼網址（例：{DefaultUrl}/api/ThirdParty/...），
    /// 只能是「協定＋主機」，不能帶路徑、參數，結尾也不能有斜線。格式不符回傳 null。
    /// </summary>
    private static string? ToSiteRoot(string? url)
    {
        if (!Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var uri))
            return null;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return null;
        if (uri.AbsolutePath != "/" || uri.Query.Length > 0 || uri.Fragment.Length > 0)
            return null;

        return uri.GetLeftPart(UriPartial.Authority);   // https://www.example.com.tw，不含結尾斜線
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}