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

/// <summary>網站的一筆網址。IsPrimary＝後台 Website.DefaultUrl；其餘存在 WebsiteAlternateUrls。</summary>
public sealed record WebsiteUrlDto(
    string Url,
    bool IsPrimary,
    DomainSummaryDto? Domain);   // null＝後台舊資料沒比對過網域

public sealed record WebsiteDetailDto
{
    /// <summary>後台 Websites.Id。</summary>
    public long Id { get; init; }
    /// <summary>站台代碼，唯讀顯示用。</summary>
    public string OrgName { get; init; } = string.Empty;
    /// <summary>0＝後台還沒綁公司。</summary>
    public long FK_CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public WebsiteLevelEnum Level { get; init; }
    public string Locale { get; init; } = string.Empty;
    public string? HostLocation { get; init; }
    public DateTime? ServiceStartDate { get; init; }
    public DateTime? ServiceEndDate { get; init; }
    public PlatformWebsiteStatusEnum Status { get; init; }
    public DateTime? TerminatedDate { get; init; }
    public bool IsDomainPending { get; init; }

    /// <summary>主要網址排第一，其他網址依排序；網域待申請時為空清單。</summary>
    public IReadOnlyList<WebsiteUrlDto> Urls { get; init; } = [];

    public string? Remark { get; init; }

    /// <summary>null＝後台還沒綁公司。</summary>
    public WebsiteCustomerDto? Customer { get; init; }
}


// ───────────────────────── 送出 ─────────────────────────

public class WebsiteSaveRequest
{
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
    public DateTime? TerminatedDate { get; set; }

    public bool IsDomainPending { get; set; }

    /// <summary>
    /// 網站的所有網址，必須剛好一筆 IsPrimary；待申請時必須是空清單。
    /// 後端依每筆網址比對網域，前端不送 FK。
    /// </summary>
    public List<WebsiteUrlInput> Urls { get; set; } = [];

    [StringLength(2000)]
    public string? Remark { get; set; }
}

public sealed class WebsiteUrlInput
{
    [StringLength(500, ErrorMessage = "網址不可超過 500 個字元。")]
    public string? Url { get; set; }

    /// <summary>true＝寫入後台 Website.DefaultUrl。</summary>
    public bool IsPrimary { get; set; }
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