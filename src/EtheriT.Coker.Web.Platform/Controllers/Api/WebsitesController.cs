using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Domains;
using EtheriT.Coker.Web.Platform.Models.Provisioning;
using EtheriT.Coker.Web.Platform.Models.Websites;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

// 網站資料直接讀寫後台 Websites；PlatformWebsites 已停用，僅保留作為搬移備份。
// 新增只建立 Website 與客戶綁定；目錄、商店設定、權限仍由後台人工補齊。
// 網址：前端把主要網址與其他網址放在同一份清單，勾選的那筆寫入 Website.DefaultUrl，
//       其餘寫入 WebsiteAlternateUrls（只有 Platform 用）。DefaultUrl 會拿來組金流／物流回呼網址，
//       編輯時只能換成主機已啟用的既有網址，見 ValidatePrimaryChangeAsync。
// 不提供 DELETE：網站以「註銷」取代刪除。
[ApiController]
[Route("api/websites")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = EtheriT.Coker.Authentication.Backoffice.BackofficeAuthorizationPolicies.PlatformDataManagement)]
public sealed class WebsitesController(
    CokerDbContext db,
    PlatformAuditor auditor,
    IOptions<WebsiteHostingOptions> hostingOptions,
    IConfiguration configuration,
    WebsiteBindingTasks bindingTasks,
    Microsoft.AspNetCore.Authorization.IAuthorizationService authorization) : ControllerBase
{
    private const int ListLimit = 1000;

    /// <summary>一個站台最多幾筆網址（含主要網址），前端 MAX_URL_COUNT 要一致。</summary>
    private const int MaxUrlCount = 10;

    /// <summary>IIS 綁定查詢一次最多幾個主機名稱：預設網址＋其他網址上限，再留一點餘裕。</summary>
    private const int MaxIisBindingHosts = 20;

    private const string SiteRootMessage =
        "請輸入網站根網址，例：https://www.example.com.tw（不可包含路徑）。";

    /// <summary>與種子資料相同；目前沒有任何程式依 Website.Type 分流。</summary>
    private const string DefaultWebsiteType = "website";

    /// <summary>
    /// OrgName 會當 upload 目錄名稱，也會直接嵌進後台寄信的正規表達式（MailAppService），
    /// 只允許小寫英數、- 與 _，且以英數開頭。
    /// </summary>
    private static readonly Regex OrgNamePattern = new("^[a-z0-9][a-z0-9_-]{1,99}$", RegexOptions.Compiled);

    /// <summary>驗證通過的網址：已轉成根網址；DomainId 為 null＝網域管理中還沒有對應的網域；IsPrimary＝主要網址。</summary>
    private sealed record ResolvedUrl(string Url, long? DomainId, bool IsPrimary);

    /// <summary>iis-bindings 回傳的 Status，前端依字串判斷。</summary>
    private static class IisBindingStatus
    {
        public const string Ok = "Ok";
        public const string NoAgent = "NoAgent";
        public const string Offline = "Offline";
        public const string Unavailable = "Unavailable";
    }

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
                // 主要網址與其他網址的網域分開取，回來再挑最早到期的
                PrimaryDomainEndDate = site.PlatformDomain == null ? (DateTime?)null : site.PlatformDomain.EndDate,
                AlternateDomainEndDate = db.WebsiteAlternateUrls
                    .Where(url => !url.IsDeleted && url.FK_WebsiteId == site.Id)
                    .Min(url => url.PlatformDomain!.EndDate),
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
                DomainEndDate: Earliest(row.PrimaryDomainEndDate, row.AlternateDomainEndDate)))
            .ToList();

        return new WebsiteListResult(items, rows.Count > ListLimit);
    }

    private static int? ToRemainingDays(DateTime? endDate, DateTime today) =>
        endDate is null ? null : (endDate.Value.Date - today).Days;

    /// <summary>兩個日期取較早的；任一為 null 時回傳另一個。</summary>
    private static DateTime? Earliest(DateTime? first, DateTime? second)
    {
        if (first is null) return second;
        if (second is null) return first;
        return first <= second ? first : second;
    }

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

    /// <summary>新增／編輯頁的主機位置選項與預設值；ServerId 只給後端用，不回傳。</summary>
    [HttpGet("options")]
    public WebsiteOptionsDto GetOptions()
    {
        var hosting = hostingOptions.Value;
        // 設定檔的 key 是版本名稱（啟動驗證保證四個版本都有），回傳改成數值字串，前端直接用 Level 查
        var defaultHostByLevel = Enum.GetValues<WebsiteLevelEnum>()
            .Select(level => (Key: ((int)level).ToString(), Host: hosting.GetDefaultHost(level)))
            .Where(item => item.Host is not null)
            .ToDictionary(item => item.Key, item => item.Host!);

        return new WebsiteOptionsDto(
            hosting.HostLocations.Select(item => new WebsiteHostLocationDto(item.Value, item.Text)).ToList(),
            defaultHostByLevel,
            hosting.DefaultUrlSuffix);
    }

    /// <summary>
    /// 查主機名稱在指定主機位置的 IIS 上綁在哪個站台。資料來自 Agent 心跳回報的 AppPoolMetricsJson，
    /// 不即時連 IIS；Status 不是 Ok 時 Items 為空清單，前端不可解讀成「沒綁定」。
    /// </summary>
    [HttpGet("iis-bindings")]
    public async Task<ActionResult<IisBindingLookupResult>> GetIisBindings(
        [FromQuery] string? hostLocation,
        [FromQuery] List<string>? hosts)
    {
        var location = hostLocation?.Trim();
        if (string.IsNullOrEmpty(location))
            ModelState.AddModelError(nameof(hostLocation), "請指定主機位置。");

        var inputs = hosts ?? [];
        if (inputs.Count > MaxIisBindingHosts)
            ModelState.AddModelError(nameof(hosts), $"一次最多查詢 {MaxIisBindingHosts} 個網址。");
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // 與網址存檔同一套規則轉成小寫主機名稱；IP、格式不合的略過
        var queryHosts = inputs
            .Select(input => PlatformDomainName.ToHost(input))
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        // 清單外的舊值、或沒有 Worker 的位置（NAS封存）都查不到 IIS
        var hostOption = hostingOptions.Value.HostLocations.FirstOrDefault(item => item.Value == location);
        if (hostOption is not { HasWorker: true })
            return new IisBindingLookupResult(IisBindingStatus.NoAgent, null, []);

        var serverId = hostOption.ServerId!.Trim();
        var state = await db.ProvisioningAgentStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.ServerId == serverId, HttpContext.RequestAborted);
        if (state is null)
            return new IisBindingLookupResult(IisBindingStatus.NoAgent, null, []);

        // 與監控頁（ProvisioningTasksController）同一個離線判斷
        var lastSeen = DateTime.SpecifyKind(state.LastSeenAtUtc, DateTimeKind.Utc);
        var offlineSeconds = Math.Max(15, configuration.GetValue("Provisioning:AgentOfflineSeconds", 45));
        if (lastSeen < DateTime.UtcNow.AddSeconds(-offlineSeconds))
            return new IisBindingLookupResult(IisBindingStatus.Offline, lastSeen, []);

        var siteByHost = ToSiteByHost(state.AppPoolMetricsJson);
        if (siteByHost is null)
            return new IisBindingLookupResult(IisBindingStatus.Unavailable, lastSeen, []);

        var items = queryHosts
            .Select(host => new IisBindingItemDto(host, siteByHost.GetValueOrDefault(host)))
            .ToList();
        return new IisBindingLookupResult(IisBindingStatus.Ok, lastSeen, items);
    }

    /// <summary>
    /// AppPoolMetricsJson（心跳存的 ProvisioningAppPoolMetricDto[]）→ 主機名稱對站台名稱。
    /// JSON 壞掉、或 Worker 版本太舊沒回報 SiteBindings 時回 null。同一主機名稱綁多個站台（不同 port）時取第一個。
    /// </summary>
    private static Dictionary<string, string>? ToSiteByHost(string? json)
    {
        List<ProvisioningAppPoolMetricDto>? pools;
        try
        {
            pools = string.IsNullOrWhiteSpace(json)
                ? []
                : JsonSerializer.Deserialize<List<ProvisioningAppPoolMetricDto>>(json);
        }
        catch (JsonException)
        {
            return null;
        }

        pools ??= [];
        if (pools.Count > 0 && pools.All(pool => pool.SiteBindings is null))
            return null;

        var siteByHost = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var binding in pools.SelectMany(pool => pool.SiteBindings ?? []))
            foreach (var hostName in binding.HostNames ?? [])
                if (PlatformDomainName.ToHost(hostName) is { } host)
                    siteByHost.TryAdd(host, binding.SiteName);

        return siteByHost;
    }

    [HttpPost]
    public async Task<ActionResult<WebsiteDetailDto>> Create(WebsiteCreateRequest request)
    {
        var orgName = request.OrgName.Trim().ToLowerInvariant();
        if (!OrgNamePattern.IsMatch(orgName))
            ModelState.AddModelError(nameof(request.OrgName),
                "站台代碼只能使用小寫英文、數字、- 與 _，長度 2～100 字，且須以英文或數字開頭。");

        // 前端第一列預設是依站台代碼組出的網址，使用者可改勾別筆；新站台至少要有一筆主要網址
        if ((request.Urls ?? []).Count == 0)
            ModelState.AddModelError(nameof(request.Urls), "請至少輸入一筆網址。");

        var urls = await ValidateRequestAsync(request, site: null, existingAlternates: []);
        if (ModelState.IsValid)
            await ValidateUrlsNotTakenAsync(urls);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var primary = urls.Single(url => url.IsPrimary);

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
                Type = DefaultWebsiteType,
                // 勾選的那筆是主要網址，其餘是其他網址
                DefaultUrl = primary.Url,
                FK_PlatformDomainId = primary.DomainId
            };
            Apply(request, site);
            db.Websites.Add(site);
            SyncAlternateUrls(site, urls.Where(url => !url.IsPrimary).ToList(), existing: []);
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
        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<ActionResult<WebsiteDetailDto>>(async () =>
        {
        db.ChangeTracker.Clear();
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, HttpContext.RequestAborted);
        var site = await db.Websites
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, HttpContext.RequestAborted);
        if (site is null)
            return NotFound();

        // 先載入其他網址：驗證要知道哪些 http 網址是原本就有的，存檔也拿它來比對
        var alternates = await db.WebsiteAlternateUrls
            .Where(item => !item.IsDeleted && item.FK_WebsiteId == site.Id)
            .ToListAsync(HttpContext.RequestAborted);

            var urls = await ValidateRequestAsync(request, site, alternates);
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            // 勾選的寫 DefaultUrl、其餘寫其他網址；舊資料的 DefaultUrl 可能帶結尾斜線，轉成根網址再比
            var primary = urls.FirstOrDefault(url => url.IsPrimary);
            var others = urls.Where(url => !url.IsPrimary).ToList();
            var primaryChanged = !string.Equals(primary?.Url, ToSiteRoot(site.DefaultUrl), StringComparison.OrdinalIgnoreCase);
            if (primaryChanged)
            {
                await ValidatePrimaryChangeAsync(site, primary, urls, alternates, request.BindingTarget);
                if (!ModelState.IsValid)
                    return ValidationProblem(ModelState);
            }

            string[] Hosts(IEnumerable<string> values) => values.Select(PlatformDomainName.ToHost).OfType<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
            var previousHosts = Hosts(alternates.Select(x => x.Url).Append(site.DefaultUrl ?? ""));
            var desiredHosts = Hosts(urls.Select(x => x.Url));
            // 換主要網址也算網址變更（要有主機權限、送一次繫結同步）；原本沒有主要網址的舊資料找不到 IIS 站台，不送
            var urlsChanged = (primaryChanged && site.DefaultUrl is not null)
                || !alternates.Select(x => x.Url).OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .SequenceEqual(others.Select(x => x.Url).OrderBy(x => x, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
            if (urlsChanged || request.SynchronizeBindings)
        {
            if (!(await authorization.AuthorizeAsync(User, null,
                EtheriT.Coker.Authentication.Backoffice.BackofficeAuthorizationPolicies.PlatformServerControl)).Succeeded)
            {
                ModelState.AddModelError(nameof(request.Urls), "您沒有變更網站網址的權限，請聯絡管理員。");
                return ValidationProblem(ModelState);
            }
            try
            {
                db.ProvisioningTasks.Add(await bindingTasks.CreateAsync(site, request.HostLocation,
                    previousHosts, desiredHosts, HttpContext.RequestAborted, request.BindingTarget));
            }
            catch (Exception ex) when (ex is InvalidOperationException or JsonException)
            {
                ModelState.AddModelError(nameof(request.Urls), ex is JsonException ? "Worker 繫結資料無法辨識，請稍後重試。" : ex.Message);
                return ValidationProblem(ModelState);
            }
        }

            Apply(request, site);
            if (primary is not null)
            {
                // 一定要在 bindingTasks.CreateAsync 之後才改：它用原本的 DefaultUrl 找 IIS 站台
                if (primaryChanged)
                    site.DefaultUrl = primary.Url;
                // 與其他網址一致，每次存檔重新比對網域
                site.FK_PlatformDomainId = primary.DomainId;
            }
            SyncAlternateUrls(site, others, alternates);

            if (!await SyncCompanyMappingAsync(site.Id, request.FK_CompanyId))
            return ValidationProblem(ModelState);

        await auditor.SaveChangesAsync(HttpContext.RequestAborted);

        var detail = await LoadDetailAsync(site.Id);
        if (detail is null)
            return NotFound();

        await transaction.CommitAsync(HttpContext.RequestAborted);
        return detail;
        });
    }

    [HttpGet("{id:long}/iis-comparison")]
    public async Task<ActionResult<WebsiteBindingComparison>> GetBindingComparison(long id, [FromQuery] string? target = null)
    {
        var site = await db.Websites.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, HttpContext.RequestAborted);
        if (site == null) return NotFound();
        return await bindingTasks.CompareAsync(site, HttpContext.RequestAborted, target);
    }

    [HttpPost("{id:long}/refresh-urls")]
    public async Task<ActionResult<WebsiteBindingComparison>> RefreshUrls(long id, [FromBody] WebsiteBindingSelection selection)
    {
        return await db.Database.CreateExecutionStrategy().ExecuteAsync<ActionResult<WebsiteBindingComparison>>(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, HttpContext.RequestAborted);
            var site = await db.Websites.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, HttpContext.RequestAborted);
            if (site == null) return NotFound();
            try
            {
                var comparison = await bindingTasks.ImportExistingUrlsAsync(site, HttpContext.RequestAborted, selection.Target);
                await auditor.SaveChangesAsync(HttpContext.RequestAborted);
                await transaction.CommitAsync(HttpContext.RequestAborted);
                return comparison;
            }
            catch (WebsiteBindingTasks.SelectionRequiredException ex)
            {
                return new WebsiteBindingComparison("SelectionRequired", ex.Message, null, null, [], [], ex.Candidates);
            }
            catch (Exception ex) when (ex is InvalidOperationException or JsonException)
            {
                return new WebsiteBindingComparison("Unavailable", ex is JsonException ? "目前無法取得網址狀態，請稍後再試。" : ex.Message,
                    null, null, [], []);
            }
        });
    }

    private async Task<WebsiteDetailDto?> LoadDetailAsync(long id)
    {
        var detail = await (
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
                DefaultUrl = site.DefaultUrl,
                DefaultDomain = site.PlatformDomain == null
                    ? null
                    : new DomainSummaryDto(
                        site.PlatformDomain.Id,
                        site.PlatformDomain.DomainName,
                        site.PlatformDomain.Registrar,
                        site.PlatformDomain.EndDate,
                        site.PlatformDomain.PasswordCipher != null,
                        site.PlatformDomain.IsProxyPurchase),
                FK_CompanyId = companyId ?? 0,
                Name = site.Title,
                Level = site.Level,
                Locale = site.Locale,
                HostLocation = site.HostLocation,
                ServiceStartDate = site.StartDate,
                ServiceEndDate = site.EndDate,
                Status = site.ServiceStatus,
                SuspendedDate = site.SuspendedDate,
                TerminatedDate = site.TerminatedDate,
                IsDomainPending = site.IsDomainPending,
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

        if (detail is null)
            return null;

        return detail with { Urls = await LoadUrlsAsync(id) };
    }

    /// <summary>其他網址（WebsiteAlternateUrls）依排序；預設網址另放在 WebsiteDetailDto.DefaultUrl。</summary>
    private async Task<IReadOnlyList<WebsiteUrlDto>> LoadUrlsAsync(long siteId) =>
        await db.WebsiteAlternateUrls
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.FK_WebsiteId == siteId)
            .OrderBy(item => item.Sort)
            .ThenBy(item => item.Id)
            .Select(item => new WebsiteUrlDto(
                item.Url,
                item.PlatformDomain == null
                    ? null
                    : new DomainSummaryDto(
                        item.PlatformDomain.Id,
                        item.PlatformDomain.DomainName,
                        item.PlatformDomain.Registrar,
                        item.PlatformDomain.EndDate,
                        item.PlatformDomain.PasswordCipher != null,
                        item.PlatformDomain.IsProxyPurchase)))
            .ToListAsync(HttpContext.RequestAborted);

    /// <summary>
    /// 回傳驗證通過的網址（含主要網址），錯誤一律寫進 ModelState。
    /// site：編輯中的站台（新增時為 null），主機位置等於它原本的值時放行。
    /// existingAlternates：這個站台原本的其他網址；它們與原本的主要網址若是 http 可原樣保留。
    /// </summary>
    private async Task<IReadOnlyList<ResolvedUrl>> ValidateRequestAsync(
        WebsiteSaveRequest request,
        Website? site,
        IReadOnlyList<WebsiteAlternateUrl> existingAlternates)
    {
        // ASP.NET 預設不驗 enum 範圍，Status: 99 會原樣寫進資料庫
        if (!Enum.IsDefined(request.Status))
            ModelState.AddModelError(nameof(request.Status), "網站狀態不正確。");

        // 必填由 [Required] 擋，這裡只驗值是否合法
        if (request.Level.HasValue && !Enum.IsDefined(request.Level.Value))
            ModelState.AddModelError(nameof(request.Level), "網站版本不正確。");

        if (!string.IsNullOrWhiteSpace(request.Locale) && WebsiteLocales.Normalize(request.Locale) is null)
            ModelState.AddModelError(nameof(request.Locale), "語系不正確。");

        ValidateHostLocation(request.HostLocation, site);

        // 暫停／註銷才准填各自的日期，且該狀態時必填；前端隱藏欄位只是體驗，這裡才是真的擋
        var isSuspended = request.Status == PlatformWebsiteStatusEnum.暫停;
        if (isSuspended && request.SuspendedDate is null)
            ModelState.AddModelError(nameof(request.SuspendedDate), "網站狀態為「暫停」時，必須填寫暫停日期。");
        else if (!isSuspended && request.SuspendedDate is not null)
            ModelState.AddModelError(nameof(request.SuspendedDate), "只有網站狀態為「暫停」時才能填寫暫停日期。");

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

        // 這個站台原本就有的 http 網址才放行（含原本的主要網址）；新網址只收 https
        var savedUrls = existingAlternates
            .Select(item => item.Url)
            .Append(ToSiteRoot(site?.DefaultUrl))
            .OfType<string>()
            .ToList();
        var allowedHttpUrls = savedUrls
            .Where(url => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // JSON 送 "Urls": null 時不會套用屬性預設值；舊資料超過上限時以原本筆數為上限
        return await ResolveUrlsAsync(request.Urls ?? [], allowedHttpUrls, Math.Max(MaxUrlCount, savedUrls.Count));
    }

    /// <summary>
    /// 主機位置必須是 WebsiteHosting:HostLocations 的 Value（Ordinal 完全比對，DB 存的就是這串字）；
    /// 編輯時等於原本的值（舊資料是 null 或清單外字串）也放行，不擋存檔。
    /// </summary>
    private void ValidateHostLocation(string? input, Website? site)
    {
        var value = Clean(input);
        if (site is not null && value == Clean(site.HostLocation))
            return;

        if (value is null)
            ModelState.AddModelError(nameof(WebsiteSaveRequest.HostLocation), "請選擇主機位置。");
        else if (!hostingOptions.Value.HostLocations.Any(item => item.Value == value))
            ModelState.AddModelError(nameof(WebsiteSaveRequest.HostLocation), "主機位置不在可選清單中。");
    }

    /// <summary>
    /// 新增站台用：網址不可已被其他站台的主要網址或其他網址使用
    /// （站台代碼 a_b 與 a-b 會組出同一個預設網址）。錯誤掛在 Urls[i].Url；只在其他驗證都通過後呼叫，索引才對得上。
    /// </summary>
    private async Task ValidateUrlsNotTakenAsync(IReadOnlyList<ResolvedUrl> urls)
    {
        var cancellationToken = HttpContext.RequestAborted;
        for (var index = 0; index < urls.Count; index++)
        {
            var url = urls[index].Url;
            var isTaken =
                await db.Websites.AnyAsync(site => !site.IsDeleted && site.DefaultUrl == url, cancellationToken) ||
                await db.WebsiteAlternateUrls.AnyAsync(item => !item.IsDeleted && item.Url == url, cancellationToken);
            if (isTaken)
                ModelState.AddModelError($"{nameof(WebsiteSaveRequest.Urls)}[{index}].{nameof(WebsiteUrlInput.Url)}",
                    $"網址「{url}」已被其他站台使用。");
        }
    }

    /// <summary>
    /// 編輯時換主要網址（Website.DefaultUrl）的額外規則，錯誤寫進 ModelState：
    /// 1. IIS 繫結同步靠「原本的主要網址」找站台，所以換的這次原主要網址要留在清單（降為其他網址），下次存檔才能刪。
    /// 2. DefaultUrl 會拿來組金流／物流回呼網址，新的主要網址必須是已存過、且主機已啟用的網址。
    /// 原本沒有主要網址的舊資料沒有可比對的主機，直接放行。只在其他驗證都通過後呼叫，索引才對得上。
    /// </summary>
    private async Task ValidatePrimaryChangeAsync(
        Website site,
        ResolvedUrl? primary,
        IReadOnlyList<ResolvedUrl> urls,
        IReadOnlyList<WebsiteAlternateUrl> existingAlternates,
        string? bindingTarget)
    {
        const string listKey = nameof(WebsiteSaveRequest.Urls);
        var previousHost = PlatformDomainName.ToHost(site.DefaultUrl);
        if (previousHost is null)
            return;

        if (!urls.Any(url => string.Equals(PlatformDomainName.ToHost(url.Url), previousHost, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(listKey,
                $"更換主要網址時，原主要網址「{site.DefaultUrl}」要先留在清單中；換好並儲存後，下次再刪除。");
            return;
        }

        if (primary is null)
            return;

        var key = $"{listKey}[{urls.ToList().IndexOf(primary)}].{nameof(WebsiteUrlInput.Url)}";
        if (!existingAlternates.Any(item => string.Equals(item.Url, primary.Url, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(key, "新加入的網址要先儲存、在主機啟用後，才能設為主要網址。");
            return;
        }

        var comparison = await bindingTasks.CompareAsync(site, HttpContext.RequestAborted, bindingTarget);
        var primaryHost = PlatformDomainName.ToHost(primary.Url)!;
        if (comparison.Status != IisBindingStatus.Ok ||
            !comparison.SiteHosts.Contains(primaryHost, StringComparer.OrdinalIgnoreCase))
            ModelState.AddModelError(key,
                $"網址「{primary.Url}」尚未在主機啟用，不能設為主要網址；請等網址狀態顯示「主機已設定此網址」後再換。");
    }

    /// <summary>
    /// 網址逐筆轉成根網址、檢查重複、比對網域；可以是 0 筆，有網址時必須剛好一筆主要網址。
    /// 單筆錯誤掛在 Urls[i].Url，前端才能對到那一列；整體錯誤掛在 Urls。
    /// </summary>
    private async Task<IReadOnlyList<ResolvedUrl>> ResolveUrlsAsync(
        IReadOnlyList<WebsiteUrlInput> inputs,
        IReadOnlySet<string> allowedHttpUrls,
        int maximum = MaxUrlCount)
    {
        const string listKey = nameof(WebsiteSaveRequest.Urls);
        if (inputs.Count > maximum)
        {
            ModelState.AddModelError(listKey, $"網址最多 {maximum} 筆（含主要網址）。");
            return [];
        }

        if (inputs.Count > 0 && inputs.Count(input => input.IsPrimary) != 1)
            ModelState.AddModelError(listKey, "請勾選一筆主要網址。");

        var resolved = new List<ResolvedUrl>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < inputs.Count; index++)
        {
            var key = $"{listKey}[{index}].{nameof(WebsiteUrlInput.Url)}";
            var (url, domainId, error) = await ResolveUrlAsync(inputs[index].Url, allowedHttpUrls);
            if (error is null && !seen.Add(PlatformDomainName.ToHost(url)!))
                error = $"網址「{url}」重複輸入。";

            if (error is not null)
                ModelState.AddModelError(key, error);
            else
                resolved.Add(new ResolvedUrl(url!, domainId, inputs[index].IsPrimary));
        }

        return resolved;
    }

    /// <summary>
    /// 單筆網址轉成根網址並比對網域。成功時 Error 為 null；比對不到網域時 DomainId 為 null，不算錯誤。
    /// 新網址只收 https；allowedHttpUrls 裡的（這個站台原本就有的 http 網址）原樣放行。
    /// </summary>
    private async Task<(string? Url, long? DomainId, string? Error)> ResolveUrlAsync(
        string? input,
        IReadOnlySet<string> allowedHttpUrls)
    {
        var root = ToSiteRoot(input);
        if (root is null)
            return (null, null, SiteRootMessage);
        if (root.Length > 255)
            return (null, null, "網址不可超過 255 個字元。");
        if (root.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !allowedHttpUrls.Contains(root))
            return (null, null, "新增的網址只接受 https://。");

        // 網址 → 網域由後端比對，不相信前端；網域還沒建立時照樣存檔，FK 留 null，下次存檔會重新比對
        var host = PlatformDomainName.ToHost(root);
        if (host is null)
            return (null, null, "網址格式不正確，例：https://www.example.com.tw");

        var domain = await db.PlatformDomains
            .AsNoTracking()
            .FindBestMatchAsync(host, HttpContext.RequestAborted);
        return (root, domain?.Id, null);
    }

    /// <summary>
    /// 只寫一般欄位。DefaultUrl 與 FK_PlatformDomainId 不在這裡：Create／Update 依勾選的主要網址另外寫，
    /// Update 必須等 IIS 繫結任務建立後才改（後台網站資料也能改 DefaultUrl，以最後存檔的為準）。
    /// </summary>
    private static void Apply(WebsiteSaveRequest request, Website site)
    {
        site.Title = request.Name.Trim();
        site.Level = request.Level!.Value;                        // [Required] 已擋
        site.Locale = WebsiteLocales.Normalize(request.Locale)!;  // [Required] 與格式驗證已擋
        site.StartDate = request.ServiceStartDate?.Date;
        site.EndDate = request.ServiceEndDate?.Date;
        site.ServiceStatus = request.Status;
        site.SuspendedDate = request.SuspendedDate?.Date;
        site.TerminatedDate = request.TerminatedDate?.Date;
        site.HostLocation = Clean(request.HostLocation);
        // 「網域待申請」已取消，存檔一律清掉舊資料的待申請狀態
        site.IsDomainPending = false;
        site.PlatformRemark = Clean(request.Remark);
    }

    /// <summary>
    /// 其他網址整批比對：網址相同的沿用原紀錄（只更新網域與排序）、新的新增、不在清單裡的軟刪除。
    /// urls 是空清單時等於全部移除。
    /// </summary>
    private void SyncAlternateUrls(Website site, IReadOnlyList<ResolvedUrl> urls, List<WebsiteAlternateUrl> existing)
    {
        var keptIds = new HashSet<long>();

        for (var sort = 0; sort < urls.Count; sort++)
        {
            var item = urls[sort];
            var target = existing.FirstOrDefault(row =>
                !keptIds.Contains(row.Id) &&
                string.Equals(row.Url, item.Url, StringComparison.OrdinalIgnoreCase));

            if (target is null)
            {
                target = new WebsiteAlternateUrl { Website = site };
                db.WebsiteAlternateUrls.Add(target);
            }
            else
            {
                keptIds.Add(target.Id);
            }

            target.Url = item.Url;
            target.FK_PlatformDomainId = item.DomainId;
            target.Sort = sort;
        }

        // PlatformAuditor 會把 Remove 轉成軟刪除
        foreach (var orphan in existing.Where(row => !keptIds.Contains(row.Id)))
            db.WebsiteAlternateUrls.Remove(orphan);
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
