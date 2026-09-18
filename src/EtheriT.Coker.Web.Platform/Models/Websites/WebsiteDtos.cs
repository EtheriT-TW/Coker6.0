using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.Platform.Models.Domains;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Web.Platform.Models.Websites;

// ───────────────────────── 回傳 ─────────────────────────

/// <summary>
/// 網站管理清單的一列。清單是「後台站台」與「僅合約」的聯集，所以兩個 Id 都可能為 null：
/// <list type="bullet">
/// <item>兩個都有值＝後台站台，且已建立合約資料</item>
/// <item>只有 <c>WebsiteId</c>＝後台站台，合約資料還沒建（<c>IsPending</c> 為 true，不對外顯示，僅用於導向與空值判斷）</item>
/// <item>只有 <c>PlatformWebsiteId</c>＝合約已簽，但還沒對應到有效站台</item>
/// </list>
/// 客戶被刪除時 Customer* 欄位為 null。
/// </summary>
public sealed record WebsiteListItemDto(
    string RowKey,
    long? PlatformWebsiteId,
    long? WebsiteId,
    string Name,
    string? OrgName,
    long? FK_CompanyId,
    string? CustomerName,
    string? CustomerTaxId,
    WebsiteLevelEnum? Level,
    string LevelText,
    PlatformWebsiteStatusEnum? Status,
    string StatusText,
    string? Url,
    DateTime? ServiceStartDate,
    DateTime? ServiceEndDate,
    int? RemainingDays,
    DateTime? DomainEndDate,
    bool IsPending);

public sealed record WebsiteListResult(
    IReadOnlyList<WebsiteListItemDto> Items,
    bool IsTruncated);

/// <summary>
/// 後台 Websites 的唯讀投影，供「對應站台」搜尋與顯示使用。
/// Level／Title／OrgName／Locale 在 Website 上皆為必填，故不可為 null。
/// Level 不附文字：enum.ToString() 無法轉譯成 SQL，改由前端用 websiteLevelOptions 對照。
/// </summary>
public sealed record WebsiteSiteOptionDto(
    long Id,
    string OrgName,
    string Title,
    string? DefaultUrl,
    WebsiteLevelEnum Level,
    string Locale,
    DateTime? StartDate,
    DateTime? EndDate);

public sealed record WebsiteSiteOptionResult(
    IReadOnlyList<WebsiteSiteOptionDto> Items,
    bool IsTruncated);

public sealed record WebsiteCustomerDto(
    long Id,
    string Name,
    string TaxId,
    string? Phone,
    string? Email,
    string? PrimaryContactName);

public sealed record WebsiteDetailDto
{
    public long Id { get; init; }
    public long FK_CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public WebsiteLevelEnum? Level { get; init; }
    public string? HostLocation { get; init; }
    public DateTime? ServiceStartDate { get; init; }
    public DateTime? ServiceEndDate { get; init; }
    public PlatformWebsiteStatusEnum Status { get; init; }
    public DateTime? TerminatedDate { get; init; }
    public bool IsDomainPending { get; init; }
    public string? Url { get; init; }

    /// <summary>null＝網域待申請或未填網址。</summary>
    public DomainSummaryDto? Domain { get; init; }
    public string? Remark { get; init; }

    /// <summary>對應的實際站台 Id；null＝尚未綁定。</summary>
    public long? FK_WebsiteId { get; init; }

    /// <summary>null＝尚未綁定，或綁定的站台已被刪除。</summary>
    public WebsiteSiteOptionDto? LinkedSite { get; init; }

    /// <summary>null＝原客戶已被刪除。</summary>
    public WebsiteCustomerDto? Customer { get; init; }
}


// ───────────────────────── 送出 ─────────────────────────

public sealed class WebsiteSaveRequest
{
    [Range(1, long.MaxValue, ErrorMessage = "請先以統一編號或公司名稱帶出客戶。")]
    public long FK_CompanyId { get; set; }

    [Required(ErrorMessage = "請輸入網站名稱。")]
    [StringLength(250, ErrorMessage = "網站名稱不可超過 250 個字元。")]
    public string Name { get; set; } = string.Empty;

    public WebsiteLevelEnum? Level { get; set; }

    [StringLength(200)]
    public string? HostLocation { get; set; }
    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }

    public PlatformWebsiteStatusEnum Status { get; set; }
    public DateTime? TerminatedDate { get; set; }

    public bool IsDomainPending { get; set; }

    /// <summary>後端依網址比對網域，前端不送 FK。</summary>
    [StringLength(500, ErrorMessage = "網址不可超過 500 個字元。")]
    public string? Url { get; set; }

    [StringLength(2000)]
    public string? Remark { get; set; }

    /// <summary>對應的實際站台 Id；null＝不綁定。存在性與重複綁定由後端驗證。</summary>
    public long? FK_WebsiteId { get; set; }
}