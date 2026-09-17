using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.Platform.Models.Domains;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Web.Platform.Models.Websites;

// ───────────────────────── 回傳 ─────────────────────────

/// <summary>客戶被刪除時 Customer* 欄位為 null。</summary>
public sealed record WebsiteListItemDto(
    long Id,
    string Name,
    long FK_CompanyId,
    string? CustomerName,
    string? CustomerTaxId,
    WebsiteLevelEnum? Level,
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
}