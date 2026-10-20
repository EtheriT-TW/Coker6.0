using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.Platform.Models.Domains;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Web.Platform.Models.Websites;

// ───────────────────────── 回傳 ─────────────────────────

/// <summary>
/// 網站管理清單的一列，一列＝一個後台站台（Websites）。
/// 客戶取後台綁定的公司（MappingCompanyAndWebsites，多筆時取最早那筆）；沒綁公司時 Customer* 為 null。
/// Url 是主要網址；DomainEndDate 取主要網址與其他網址的網域中最早到期的。
/// </summary>
public sealed record WebsiteListItemDto(
    long Id,
    string Name,
    string OrgName,
    long? FK_CompanyId,
    string? CustomerName,
    string? CustomerTaxId,
    WebsiteLevelEnum Level,
    string LevelText,
    PlatformWebsiteStatusEnum Status,
    string StatusText,
    string? Url,
    DateTime? ServiceStartDate,
    DateTime? ServiceEndDate,
    int? RemainingDays,
    DateTime? DomainEndDate);

public sealed record WebsiteListResult(
    IReadOnlyList<WebsiteListItemDto> Items,
    bool IsTruncated);

public sealed record WebsiteCustomerDto(
    long Id,
    string Name,
    string TaxId,
    string? Phone,
    string? Email,
    string? PrimaryContactName);

/// <summary>網站的一筆其他網址（WebsiteAlternateUrls）。</summary>
public sealed record WebsiteUrlDto(
    string Url,
    DomainSummaryDto? Domain);   // null＝網域管理中沒有對應的網域

public sealed record WebsiteEnterpriseEmailDto
{
    public EnterpriseEmailTypeEnum AccountType { get; init; }
    public int TotalAccounts { get; init; }
    public string? OtherType { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string? Remark { get; init; }
}

public sealed record WebsiteDetailDto
{
    /// <summary>後台 Websites.Id。</summary>
    public long Id { get; init; }
    /// <summary>站台代碼，唯讀顯示用。</summary>
    public string OrgName { get; init; } = string.Empty;
    /// <summary>後台 Website.DefaultUrl，唯讀顯示用；新增後由後台維護。</summary>
    public string? DefaultUrl { get; init; }
    /// <summary>0＝後台還沒綁公司。</summary>
    public long FK_CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public WebsiteLevelEnum Level { get; init; }
    public string Locale { get; init; } = string.Empty;
    public string? HostLocation { get; init; }
    public DateTime? ServiceStartDate { get; init; }
    public DateTime? ServiceEndDate { get; init; }
    public PlatformWebsiteStatusEnum Status { get; init; }
    public DateTime? SuspendedDate { get; init; }
    public DateTime? TerminatedDate { get; init; }
    /// <summary>舊資料的「網域待申請」；存檔後一律變 false，前端用來判斷要不要自動補網址。</summary>
    public bool IsDomainPending { get; init; }

    /// <summary>其他網址（WebsiteAlternateUrls）依排序，不含預設網址。</summary>
    public IReadOnlyList<WebsiteUrlDto> Urls { get; init; } = [];

    public string? Remark { get; init; }

    /// <summary>null＝後台還沒綁公司。</summary>
    public WebsiteCustomerDto? Customer { get; init; }
    public WebsiteEnterpriseEmailDto? EnterpriseEmail { get; init; }
}


// ───────────────────────── 送出 ─────────────────────────

public class WebsiteSaveRequest
{
    public bool SynchronizeBindings { get; set; }
    [Range(1, long.MaxValue, ErrorMessage = "請先以統一編號或公司名稱帶出客戶。")]
    public long FK_CompanyId { get; set; }

    [Required(ErrorMessage = "請輸入網站名稱。")]
    [StringLength(250, ErrorMessage = "網站名稱不可超過 250 個字元。")]
    public string Name { get; set; } = string.Empty;

    /// <summary>寫入 Website.Level，必填。</summary>
    [Required(ErrorMessage = "請選擇網站版本。")]
    public WebsiteLevelEnum? Level { get; set; }

    /// <summary>zh-tw／en，寫入 Website.Locale，必填。</summary>
    [Required(ErrorMessage = "請選擇語系。")]
    [StringLength(10)]
    public string? Locale { get; set; }

    [StringLength(200)]
    public string? HostLocation { get; set; }
    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }

    public PlatformWebsiteStatusEnum Status { get; set; }
    public DateTime? SuspendedDate { get; set; }
    public DateTime? TerminatedDate { get; set; }

    /// <summary>
    /// 只放「其他網址」（WebsiteAlternateUrls），可以是 0 筆，不可與預設網址重複。
    /// 預設網址（Website.DefaultUrl）新增時由後端依 OrgName 組出，編輯時不寫入。
    /// 後端依每筆網址比對網域，前端不送 FK。
    /// </summary>
    public List<WebsiteUrlInput> Urls { get; set; } = [];

    public WebsiteEnterpriseEmailInput? EnterpriseEmail { get; set; }

    [StringLength(2000)]
    public string? Remark { get; set; }
}

public sealed class WebsiteUrlInput
{
    [StringLength(500, ErrorMessage = "網址不可超過 500 個字元。")]
    public string? Url { get; set; }
}

/// <summary>
/// 新增站台用。OrgName 建立後不可修改：upload 目錄、前台站台設定都綁著它。
/// 格式與重複檢查在 WebsitesController.Create。
/// </summary>
public sealed class WebsiteCreateRequest : WebsiteSaveRequest
{
    [Required(ErrorMessage = "請輸入站台代碼。")]
    [StringLength(100, ErrorMessage = "站台代碼不可超過 100 個字元。")]
    public string OrgName { get; set; } = string.Empty;
}

public sealed class WebsiteEnterpriseEmailInput {
    [Required(ErrorMessage= "請選擇帳號版本。")]
    public EnterpriseEmailTypeEnum? AccountType { get; set; }
    [Required(ErrorMessage = "請輸入帳號總數。")]
    [Range(1, long.MaxValue, ErrorMessage= "帳號總數至少為 1。")]
    public int? TotalAccounts { get; set; }
    [StringLength(200, ErrorMessage= "不可超過 200 個字元。")]
    public string? OtherType { get; set; }
    [Required(ErrorMessage = "請輸入到期日期。")]
    public DateTime? ExpirationDate { get; set; }
    [StringLength(2000, ErrorMessage = "備註不可超過 2000 個字元。")]
    public string? Remark { get; set; }
}

// ───────────────────────── 選項與 IIS 綁定查詢 ─────────────────────────

public sealed record WebsiteHostLocationDto(string Value, string Text);

/// <summary>GET /api/websites/options。DefaultHostByLevel 的 key 是 WebsiteLevelEnum 的數值字串。</summary>
public sealed record WebsiteOptionsDto(
    IReadOnlyList<WebsiteHostLocationDto> HostLocations,
    IReadOnlyDictionary<string, string> DefaultHostByLevel,
    string DefaultUrlSuffix);

/// <summary>SiteName 為 null＝這個主機名稱在該主機的 IIS 上沒有綁定。</summary>
public sealed record IisBindingItemDto(string Host, string? SiteName);

/// <summary>
/// GET /api/websites/iis-bindings。Status：Ok／NoAgent／Offline／Unavailable；
/// 不是 Ok 時 Items 為空清單，不可解讀成「沒綁定」。
/// </summary>
public sealed record IisBindingLookupResult(
    string Status,
    DateTime? LastSeenAtUtc,
    IReadOnlyList<IisBindingItemDto> Items);

public sealed record WebsiteBindingComparison(string Status, string? Message, string? SiteName,
    DateTime? LastSeenAtUtc, IReadOnlyList<string> SiteHosts, IReadOnlyList<IisBindingItemDto> OtherBindings);
