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
    string? PrimaryContactName,
    bool HasEnterpriseEmail,
    string? Urls,
    string? Domains);

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

/// <summary>客戶編輯頁「所屬站台」的一列＝後台綁到這個客戶的一個站台（MappingCompanyAndWebsites）。</summary>
public sealed record CustomerWebsiteDto(
    long WebsiteId,
    string Name,
    string OrgName,
    WebsiteLevelEnum Level,
    string LevelText,
    PlatformWebsiteStatusEnum Status,
    string StatusText,
    string? Url,
    DateTime? ServiceStartDate,
    DateTime? ServiceEndDate,
    int? RemainingDays);

/// <summary>客戶的企業信箱（WebsiteEnterpriseEmails），一個客戶最多一筆。</summary>
public sealed record EnterpriseEmailDto
{
    public EnterpriseEmailTypeEnum AccountType { get; init; }
    public int TotalAccounts { get; init; }
    public string? OtherType { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string? Remark { get; init; }
}

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
    public string? Remark { get; init; }
    /// <summary>null＝未使用企業信箱。</summary>
    public EnterpriseEmailDto? EnterpriseEmail { get; init; }
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
    [StringLength(2000, ErrorMessage  = "備註總字數不可超過2000字元")]
    public string? Remark { get; set; }

    public List<CustomerContactSaveRequest> SubContacts { get; set; } = [];

    /// <summary>null＝未使用；原本有的會被軟刪除。</summary>
    public EnterpriseEmailInput? EnterpriseEmail { get; set; }

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

/// <summary>錯誤 key 會是 EnterpriseEmail.xxx，前端 getErrors('EnterpriseEmail.xxx') 對得上。</summary>
public sealed class EnterpriseEmailInput : IValidatableObject
{
    [Required(ErrorMessage = "請選擇帳號版本。")]
    public EnterpriseEmailTypeEnum? AccountType { get; set; }

    [Required(ErrorMessage = "請輸入帳號總數。")]
    [Range(1, int.MaxValue, ErrorMessage = "帳號總數至少為 1。")]
    public int? TotalAccounts { get; set; }

    [StringLength(200, ErrorMessage = "不可超過 200 個字元。")]
    public string? OtherType { get; set; }

    [Required(ErrorMessage = "請輸入到期日期。")]
    public DateTime? ExpirationDate { get; set; }

    [StringLength(2000, ErrorMessage = "備註不可超過 2000 個字元。")]
    public string? Remark { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // ASP.NET 預設不驗 enum 範圍，99 會原樣寫進資料庫
        if (AccountType.HasValue && !Enum.IsDefined(AccountType.Value))
            yield return new ValidationResult("帳號版本不正確。", [nameof(AccountType)]);

        if (AccountType == EnterpriseEmailTypeEnum.Other && string.IsNullOrWhiteSpace(OtherType))
            yield return new ValidationResult("請輸入自訂版本名稱。", [nameof(OtherType)]);
    }
}