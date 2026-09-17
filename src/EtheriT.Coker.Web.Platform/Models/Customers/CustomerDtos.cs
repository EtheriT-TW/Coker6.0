using EtheriT.Coker.Application.Shared.Dto.enumType;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Web.Platform.Models.Customers;

// ───────────────────────── 回傳 ─────────────────────────

public sealed record CustomerListItemDto(
    long Id,
    string Name,
    string TaxId,
    string? Phone,
    string? Email,
    string? SalesOwner,
    string CustomerTypeText,
    string? PrimaryContactName);

public sealed record CustomerContactDto(
    long Id,
    string Name,
    string? JobTitle,
    string? Phone,
    string? Email,
    int Sort);

public sealed record CustomerLookupDto(
    long Id,
    string Name,
    string TaxId,
    string? Phone,
    string? Email,
    string? PrimaryContactName);

public sealed record CustomerDetailDto
{
    public long Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string TaxId { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? InvoiceInfo { get; init; }
    public string? SalesOwner { get; init; }
    public PlatformCustomerTypeEnum CustomerType { get; init; }
    public string? CustomerTypeOther { get; init; }
    public string? PrimaryContactName { get; init; }
    public string? PrimaryContactJobTitle { get; init; }
    public string? PrimaryContactPhone { get; init; }
    public string? PrimaryContactEmail { get; init; }
    public IReadOnlyList<CustomerContactDto> SubContacts { get; init; } = [];
}

// ───────────────────────── 送出 ─────────────────────────

public sealed class CustomerSaveRequest : IValidatableObject
{
    [Required(ErrorMessage = "請輸入公司名稱。")]
    [StringLength(200, ErrorMessage = "公司名稱不可超過 200 個字元。")]
    public string Name { get; set; } = string.Empty;
    // 選填；Companies.TaxID 欄位長度 10
    [RegularExpression(@"^\d{8,10}$", ErrorMessage = "統一編號需為 8～10 碼數字。")]
    public string? TaxId { get; set; }

    [StringLength(50)] 
    public string? Phone { get; set; }

    [StringLength(150)]
    [OptionalEmail(ErrorMessage = "公司 Email 格式不正確。")]
    public string? Email { get; set; }

    [StringLength(150, ErrorMessage = "公司地址不可超過 150 個字元。")]
    public string? Address { get; set; }
    [StringLength(500)] 
    public string? InvoiceInfo { get; set; }
    [StringLength(50)] 
    public string? SalesOwner { get; set; }

    public PlatformCustomerTypeEnum CustomerType { get; set; }
    [StringLength(50)] 
    public string? CustomerTypeOther { get; set; }

    [Required(ErrorMessage = "請輸入主要聯絡人姓名。")]
    [StringLength(100)]
    public string PrimaryContactName { get; set; } = string.Empty;

    [StringLength(100)] 
    public string? PrimaryContactJobTitle { get; set; }
    [StringLength(50)] 
    public string? PrimaryContactPhone { get; set; }

    [StringLength(150)]
    [OptionalEmail(ErrorMessage = "聯絡人 Email 格式不正確。")]
    public string? PrimaryContactEmail { get; set; }

    public List<CustomerContactSaveRequest> SubContacts { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CustomerType == PlatformCustomerTypeEnum.未設定)
        {
            yield return new ValidationResult(
                "請選擇客戶屬性。", [nameof(CustomerType)]);
        }

        if (CustomerType == PlatformCustomerTypeEnum.其他 &&
            string.IsNullOrWhiteSpace(CustomerTypeOther))
        {
            yield return new ValidationResult(
                "選擇「其他」時請輸入客戶屬性內容。", [nameof(CustomerTypeOther)]);
        }
    }
}

public sealed class CustomerContactSaveRequest
{
    /// <summary>大於 0 代表既有列；0 或負數代表新增（前端用遞減負數當暫時 key）。</summary>
    public long Id { get; set; }

    [Required(ErrorMessage = "請輸入聯絡人姓名。")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)] 
    public string? JobTitle { get; set; }
    [StringLength(50)] 
    public string? Phone { get; set; }

    [StringLength(150)]
    [OptionalEmail(ErrorMessage = "聯絡人 Email 格式不正確。")]
    public string? Email { get; set; }
}