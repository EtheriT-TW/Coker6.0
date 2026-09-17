using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Web.Platform.Models.Domains;

// ───────────────────────── 回傳 ─────────────────────────

public sealed record DomainListItemDto(
    long Id,
    string DomainName,
    string? Registrar,
    DateTime? StartDate,
    DateTime? EndDate,
    int WebsiteCount);

public sealed record DomainListResult(
    IReadOnlyList<DomainListItemDto> Items,
    bool IsTruncated);

/// <summary>刻意不含 PasswordCipher，只回 HasPassword。WebsiteCount &gt; 0 時不可改網域名稱。</summary>
public sealed record DomainDetailDto(
    long Id,
    string DomainName,
    string? Registrar,
    DateTime? StartDate,
    DateTime? EndDate,
    bool HasPassword,
    string? Remark,
    int WebsiteCount);

/// <summary>網站頁顯示用的精簡資料。</summary>
public sealed record DomainSummaryDto(
    long Id,
    string DomainName,
    string? Registrar,
    DateTime? EndDate);

/// <summary>Host＝null：網址格式不正確；Domain＝null：網域尚未建立。</summary>
public sealed record DomainMatchDto(
    string? Host,
    string? SuggestedDomainName,
    DomainSummaryDto? Domain);

public sealed record DomainPasswordDto(string State, string? Password);

// ───────────────────────── 送出 ─────────────────────────

public sealed class DomainSaveRequest
{
    [Required(ErrorMessage = "請輸入網域。")]
    [StringLength(255, ErrorMessage = "網域不可超過 255 個字元。")]
    public string DomainName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Registrar { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>明文。null＝不動既有密碼。</summary>
    [StringLength(200, ErrorMessage = "網域密碼不可超過 200 個字元。")]
    public string? Password { get; set; }

    /// <summary>true＝清空既有密碼。與 Password 互斥。</summary>
    public bool ClearPassword { get; set; }

    [StringLength(2000)]
    public string? Remark { get; set; }
}