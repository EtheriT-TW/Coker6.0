using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Customers;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

// 客戶資料直接讀寫後台的 Companies。
// ⚠ 會連動前台 SEO 結構化資料、訂單通知信 CC、後台以統編／名稱比對公司，所以統編與名稱不可重複。
[ApiController]
[Route("api/companies")]
public sealed class CustomersController(CokerDbContext db, PlatformAuditor auditor) : ControllerBase
{
    private const int ListLimit = 2000;

    [HttpGet]
    public async Task<IReadOnlyList<CustomerListItemDto>> GetList()
    {
        var rows = await db.Companies
            .AsNoTracking()
            .OrderByDescending(company => company.Id)
            .Take(ListLimit)
            .Select(company => new
            {
                company.Id,
                company.Name,
                company.TaxID,
                company.Phone,
                company.Email,
                company.SalesOwner,
                company.CustomerType,
                company.CustomerTypeOther,
                company.Contact
            })
            .ToListAsync(HttpContext.RequestAborted);

        return rows.Select(row => new CustomerListItemDto(
                row.Id,
                row.Name,
                row.TaxID,
                row.Phone,
                Clean(row.Email),
                row.SalesOwner,
                DescribeCustomerType(row.CustomerType, row.CustomerTypeOther),
                Clean(row.Contact)
            ))
            .ToList();
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CustomerDetailDto>> GetDetail(long id)
    {
        var company = await db.Companies
            .AsNoTracking()
            .Include(item => item.SecondaryContacts)
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);

        if (company is null)
            return NotFound();

        return new CustomerDetailDto
        {
            Id = company.Id,
            Name = company.Name,
            TaxId = company.TaxID,
            Phone = company.Phone,
            Email = Clean(company.Email),
            Address = Clean(company.Address),
            InvoiceInfo = company.InvoiceInfo,
            SalesOwner = company.SalesOwner,
            CustomerType = company.CustomerType,
            CustomerTypeOther = company.CustomerTypeOther,
            PrimaryContactName = Clean(company.Contact),
            PrimaryContactJobTitle = company.ContactJobTitle,
            PrimaryContactPhone = company.ContactPhone,
            PrimaryContactEmail = company.ContactEmail,
            SubContacts = company.SecondaryContacts
                .OrderBy(contact => contact.Sort)
                .Select(contact => new CustomerContactDto(
                    contact.Id,
                    contact.Name,
                    contact.JobTitle,
                    contact.Phone,
                    contact.Email,
                    contact.Sort))
                .ToList()
        };
    }

    /// <summary>網站編輯頁「帶出客戶」：統編或完整公司名稱，兩者皆唯一所以最多一筆。查無回 404。</summary>
    [HttpGet("lookup")]
    public async Task<ActionResult<CustomerLookupDto>> Lookup([FromQuery] string? keyword)
    {
        var text = NormalizeName(keyword);
        if (text.Length == 0)
            return NotFound();

        var company = await db.Companies
            .AsNoTracking()
            .Where(item => item.TaxID == text || item.Name == text)
            // 萬一有公司名稱剛好長得像別家的統編，統編優先
            .OrderBy(item => item.TaxID == text ? 0 : 1)
            .Select(item => new CustomerLookupDto(
                item.Id,
                item.Name,
                item.TaxID,
                item.Phone,
                item.Email,
                item.Contact))
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        return company is null ? NotFound() : company;
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create(CustomerSaveRequest request)
    {
        if (!await ValidateUniqueAsync(request, excludeId: null))
            return ValidationProblem(ModelState);

        var company = new Company();
        Apply(request, company);

        company.SecondaryContacts = request.SubContacts.Select((item, index) => new SecondaryContact
        {
            Name = item.Name.Trim(),
            JobTitle = Clean(item.JobTitle),
            Phone = Clean(item.Phone),
            Email = Clean(item.Email),
            Sort = index
        })
            .ToList();

        db.Companies.Add(company);
        var failure = await TrySaveAsync();
        if (failure is not null)
            return failure;

        return Ok(new { company.Id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, CustomerSaveRequest request)
    {
        var company = await db.Companies
            .Include(item => item.SecondaryContacts)
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);

        if (company is null)
            return NotFound();

        if (!await ValidateUniqueAsync(request, id))
            return ValidationProblem(ModelState);

        Apply(request, company);
        SyncSecondaryContacts(request, company);

        return await TrySaveAsync() ?? NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var company = await db.Companies
            .Include(item => item.SecondaryContacts)
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);

        if (company is null)
            return NotFound();

        // 後台已綁定站台的公司不可刪：前台 SEO 與訂單通知信 CC 都在讀它。
        var boundSiteCount = await db.MappingCompanyAndWebsites
            .CountAsync(mapping => mapping.FK_CompanyId == id, HttpContext.RequestAborted);
        if (boundSiteCount > 0)
            return Conflict(new
            {
                Error = $"此客戶已在後台綁定 {boundSiteCount} 個站台，無法刪除。"
            });

        // 網站不提供刪除，只要客戶底下有任何網站（含已註銷）就不能刪，
        // 否則網站會失去對應客戶。
        var websiteCount = await db.PlatformWebsites
            .CountAsync(site => site.FK_CompanyId == id, HttpContext.RequestAborted);
        if (websiteCount > 0)
            return Conflict(new
            {
                Error = $"此客戶底下還有 {websiteCount} 個網站，無法刪除。網站不提供刪除，請改為註銷網站並保留客戶資料。"
            });

        foreach (var contact in company.SecondaryContacts)
        {
            db.SecondaryContacts.Remove(contact);
        }
        db.Companies.Remove(company);

        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>去頭尾空白；整串空白視同未填。從 Excel 貼過來很常帶空白。</summary>
    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>與後台 CompanyAppService.NormalizeCompanyName 相同：去頭尾、連續空白收成一個，重複比對才對得上。</summary>
    private static string NormalizeName(string? name) =>
        string.Join(' ', (name ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static void Apply(CustomerSaveRequest request, Company company)
    {
        company.Name = NormalizeName(request.Name);
        // Companies 的 TaxID／Contact／Email／Address 是 NOT NULL，沒填存空字串
        company.TaxID = request.TaxId?.Trim() ?? string.Empty;
        company.Contact = request.PrimaryContactName.Trim();
        company.Email = request.Email?.Trim() ?? string.Empty;
        company.Address = request.Address?.Trim() ?? string.Empty;
        company.Phone = Clean(request.Phone);
        company.InvoiceInfo = Clean(request.InvoiceInfo);
        company.SalesOwner = Clean(request.SalesOwner);
        company.CustomerType = request.CustomerType;
        // 不是「其他」就把自填內容清掉，避免改過屬性後留下殘值。
        company.CustomerTypeOther = request.CustomerType == PlatformCustomerTypeEnum.其他
            ? Clean(request.CustomerTypeOther)
            : null;
        company.ContactJobTitle = Clean(request.PrimaryContactJobTitle);
        company.ContactPhone = Clean(request.PrimaryContactPhone);
        company.ContactEmail = Clean(request.PrimaryContactEmail);
    }

    /// <summary>統編（有填才檢查）與公司名稱不可和其他未刪除的公司重複。</summary>
    private async Task<bool> ValidateUniqueAsync(CustomerSaveRequest request, long? excludeId)
    {
        var name = NormalizeName(request.Name);
        var taxId = request.TaxId?.Trim() ?? string.Empty;
        var others = db.Companies
            .AsNoTracking()
            .Where(company => excludeId == null || company.Id != excludeId);

        if (await others.AnyAsync(company => company.Name == name, HttpContext.RequestAborted))
            ModelState.AddModelError(nameof(request.Name), "此公司名稱已被其他客戶使用。");

        if (taxId.Length > 0 &&
            await others.AnyAsync(company => company.TaxID == taxId, HttpContext.RequestAborted))
            ModelState.AddModelError(nameof(request.TaxId), "此統一編號已被其他客戶使用。");

        return ModelState.IsValid;
    }

    /// <summary>
    /// 查完重複到實際寫入之間，可能被別人搶先存了同名／同統編，
    /// 由唯一索引擋下（SQL 錯誤 2601／2627），轉成 409 而不是 500。回傳 null 代表成功。
    /// </summary>
    private async Task<ActionResult?> TrySaveAsync()
    {
        try
        {
            await auditor.SaveChangesAsync(HttpContext.RequestAborted);
            return null;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Conflict(new { Error = "公司名稱或統一編號剛被其他人使用，請重新確認後再儲存。" });
        }
    }

    /// <summary>次要聯絡人整批比對：留下的更新、Id 非正數的新增、不在清單裡的軟刪除。</summary>
    private void SyncSecondaryContacts(CustomerSaveRequest request, Company company)
    {
        var existing = company.SecondaryContacts.ToDictionary(contact => contact.Id);
        var keptIds = new HashSet<long>();

        for (var index = 0; index < request.SubContacts.Count; index++)
        {
            var item = request.SubContacts[index];
            if (item.Id > 0 && existing.TryGetValue(item.Id, out var target))
            {
                keptIds.Add(target.Id);
            }
            else
            {
                target = new SecondaryContact
                {
                    FK_CompanyId = company.Id
                };
                company.SecondaryContacts.Add(target);
            }

            target.Name = item.Name.Trim();
            target.JobTitle = Clean(item.JobTitle);
            target.Phone = Clean(item.Phone);
            target.Email = Clean(item.Email);
            target.Sort = index;
        }

        foreach (var orphan in existing.Values.Where(contact => !keptIds.Contains(contact.Id)))
        {
            db.SecondaryContacts.Remove(orphan);
        }
    }

    private static string DescribeCustomerType(PlatformCustomerTypeEnum type, string? other) => type switch
    {
        PlatformCustomerTypeEnum.其他 => string.IsNullOrWhiteSpace(other) ? "其他" : other,
        PlatformCustomerTypeEnum.未設定 => "—",
        _ => type.ToString()
    };
}