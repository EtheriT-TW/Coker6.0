using EtheriT.Coker.Application.Shared.Dto.enumType;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Web.Platform.Models.Websites;

// ───────────────────────── 回傳 ─────────────────────────

/// <summary>客戶被刪除時 Customer* 欄位為 null。</summary>
public sealed record WebsiteListItemDto(
    long Id,
    string Name,
    long FK_PlatformCustomerId,
    string? CustomerName,
    string? CustomerTaxId,
    WebsiteLevelEnum? Level,
    string LevelText,
    PlatformWebsiteStatusEnum Status,
    string StatusText,
    string? DomainName,
    DateTime? ServiceEndDate,
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

/// <summary>刻意不含 DomainPasswordCipher，只回 HasDomainPassword。</summary>
public sealed record WebsiteDetailDto
{
    public long Id { get; init; }
    public long FK_PlatformCustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public WebsiteLevelEnum? Level { get; init; }
    public string? HostLocation { get; init; }
    public DateTime? ServiceStartDate { get; init; }
    public DateTime? ServiceEndDate { get; init; }
    public PlatformWebsiteStatusEnum Status { get; init; }
    public DateTime? TerminatedDate { get; init; }
    public bool IsDomainPending { get; init; }
    public string? DomainName { get; init; }
    public string? DomainRegistrar { get; init; }
    public DateTime? DomainStartDate { get; init; }
    public DateTime? DomainEndDate { get; init; }
    public bool HasDomainPassword { get; init; }
    public string? Remark { get; init; }

    /// <summary>null＝原客戶已被刪除。</summary>
    public WebsiteCustomerDto? Customer { get; init; }
}

public sealed record DomainPasswordDto(string State, string? Password);

// ───────────────────────── 送出 ─────────────────────────

public sealed class WebsiteSaveRequest
{
    [Range(1, long.MaxValue, ErrorMessage = "請先以統一編號帶出客戶。")]
    public long FK_PlatformCustomerId { get; set; }

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
    [StringLength(255)]
    public string? DomainName { get; set; }
    [StringLength(200)]
    public string? DomainRegistrar { get; set; }
    public DateTime? DomainStartDate { get; set; }
    public DateTime? DomainEndDate { get; set; }

    /// <summary>明文。null＝不動既有密碼。</summary>
    [StringLength(200, ErrorMessage = "網域密碼不可超過 200 個字元。")]
    public string? DomainPassword { get; set; }

    /// <summary>true＝清空既有密碼。與 DomainPassword 互斥。</summary>
    public bool ClearDomainPassword { get; set; }

    [StringLength(2000)]
    public string? Remark { get; set; }
}