using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Customers;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/companies")]

public sealed class CustomersController(CokerDbContext db, PlatformAuditor auditor) : ControllerBase {
    private const int ListLimit = 2000;

    [HttpGet]
    public async Task<IReadOnlyList<CustomerListItemDto>> GetList() {
        var rows = await db.PlatformCustomers
            .AsNoTracking()
            .OrderByDescending(customer => customer.Id)
            .Take(ListLimit)
            .Select(customer => new
            {
                customer.Id,
                customer.Name,
                customer.TaxId,
                customer.Phone,
                customer.Email,
                customer.SalesOwner,
                customer.CustomerType,
                customer.CustomerTypeOther,
                customer.PrimaryContactName
            })
            .ToListAsync(HttpContext.RequestAborted);

        return rows.Select(row => new CustomerListItemDto(
                row.Id,
                row.Name,
                row.TaxId,
                row.Phone,
                row.Email,
                row.SalesOwner,
                DescribeCustomerType(row.CustomerType, row.CustomerTypeOther),
                row.PrimaryContactName
            ))
            .ToList();
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CustomerDetailDto>> GetDetail(long id) {
        var customer = await db.PlatformCustomers
            .AsNoTracking()
            .Include(item => item.SubContacts)
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);

        if(customer is null)
            return NotFound();

        return new CustomerDetailDto
        {
            Id = customer.Id,
            Name = customer.Name,
            TaxId = customer.TaxId,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            InvoiceInfo = customer.InvoiceInfo,
            SalesOwner = customer.SalesOwner,
            CustomerType = customer.CustomerType,
            CustomerTypeOther = customer.CustomerTypeOther,
            PrimaryContactName = customer.PrimaryContactName,
            PrimaryContactJobTitle = customer.PrimaryContactJobTitle,
            PrimaryContactPhone = customer.PrimaryContactPhone,
            PrimaryContactEmail = customer.PrimaryContactEmail,
            SubContacts = customer.SubContacts
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

    [HttpGet("by-tax-id")]
    public async Task<IReadOnlyList<CustomerLookupDto>> GetByTaxId([FromQuery] string? taxId, [FromQuery] long? excludeId) {
        if (string.IsNullOrWhiteSpace(taxId))
            return [];

        var text = taxId.Trim();
        return await db.PlatformCustomers
            .AsNoTracking()
            .Where(customer => customer.TaxId == text)
            .Where(customer => excludeId == null || customer.Id != excludeId)
            .OrderBy(customer => customer.Id)
            .Select(customer => new CustomerLookupDto(
                customer.Id,
                customer.Name,
                customer.TaxId,
                customer.Phone,
                customer.Email,
                customer.PrimaryContactName))
            .ToListAsync(HttpContext.RequestAborted);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create(CustomerSaveRequest request) {
        var customer = new PlatformCustomer();
        Apply(request, customer);

        customer.SubContacts = request.SubContacts.Select((item, index) => new PlatformCustomerContact
            {
                Name = item.Name.Trim(),
                JobTitle = Clean(item.JobTitle),
                Phone = Clean(item.Phone),
                Email = Clean(item.Email),
                Sort = index
            })
            .ToList();

        db.PlatformCustomers.Add(customer);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok(new { customer.Id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, CustomerSaveRequest request) {
        var customer = await db.PlatformCustomers
            .Include(item => item.SubContacts)
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);

        if(customer is null)
            return NotFound();

        Apply(request, customer);
        SyncSubContact(request, customer);

        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id) {
        var customer = await db.PlatformCustomers
            .Include(item => item.SubContacts)
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);

        // 網站不提供刪除，只要客戶底下有任何網站（含已註銷）就不能刪，
        // 否則網站會失去對應客戶。
        var websiteCount = await db.PlatformWebsites
            .CountAsync(site => site.FK_PlatformCustomerId == id, HttpContext.RequestAborted);
        if (websiteCount > 0)
            return Conflict(new
            {
                Error = $"此客戶底下還有 {websiteCount} 個網站，無法刪除。網站不提供刪除，請改為註銷網站並保留客戶資料。"
            });

        foreach (var contact in customer.SubContacts) {
            db.PlatformCustomerContacts.Remove(contact);
        }
        db.PlatformCustomers.Remove(customer);

        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>去頭尾空白；整串空白視同未填。從 Excel 貼過來很常帶空白。</summary>
    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Apply(CustomerSaveRequest request, PlatformCustomer customer)
    {
        customer.Name = request.Name.Trim();
        customer.TaxId = request.TaxId?.Trim() ?? string.Empty;
        customer.Phone = Clean(request.Phone);
        customer.Email = Clean(request.Email);
        customer.Address = Clean(request.Address);
        customer.InvoiceInfo = Clean(request.InvoiceInfo);
        customer.SalesOwner = Clean(request.SalesOwner);
        customer.CustomerType = request.CustomerType;
        // 不是「其他」就把自填內容清掉，避免改過屬性後留下殘值。
        customer.CustomerTypeOther = request.CustomerType == PlatformCustomerTypeEnum.其他
            ? Clean(request.CustomerTypeOther)
            : null;
        customer.PrimaryContactName = Clean(request.PrimaryContactName);
        customer.PrimaryContactJobTitle = Clean(request.PrimaryContactJobTitle);
        customer.PrimaryContactPhone = Clean(request.PrimaryContactPhone);
        customer.PrimaryContactEmail = Clean(request.PrimaryContactEmail);
    }

    /// <summary>次要聯絡人整批比對：留下的更新、Id 非正數的新增、不在清單裡的軟刪除。</summary>
    private void SyncSubContact(CustomerSaveRequest request, PlatformCustomer customer) {
        var existing = customer.SubContacts.ToDictionary(contact => contact.Id);
        var keptIds = new HashSet<long>();

        for (var index = 0; index < request.SubContacts.Count; index++) {
            var item = request.SubContacts[index];
            if (item.Id > 0 && existing.TryGetValue(item.Id, out var target))
            {
                keptIds.Add(target.Id);
            }
            else {
                target = new PlatformCustomerContact
                {
                    FK_PlatformCustomerId = customer.Id
                };
                customer.SubContacts.Add(target);
            }

            target.Name = item.Name.Trim();
            target.JobTitle = Clean(item.JobTitle);
            target.Phone = Clean(item.Phone);
            target.Email = Clean(item.Email);
            target.Sort = index;
        }

        foreach (var orphan in existing.Values.Where(contact => !keptIds.Contains(contact.Id))) {
            db.PlatformCustomerContacts.Remove(orphan);
        }
    }

    private static string DescribeCustomerType(PlatformCustomerTypeEnum type, string? other) => type switch
    {
        PlatformCustomerTypeEnum.其他 => string.IsNullOrWhiteSpace(other) ? "其他" : other,
        PlatformCustomerTypeEnum.未設定 => "—",
        _ => type.ToString()
    };
}