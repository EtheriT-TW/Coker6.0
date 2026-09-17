using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Domains;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

// 不提供 DELETE：網站以 Restrict FK 參照網域。
[ApiController]
[Route("api/domains")]
public sealed class DomainsController(
    CokerDbContext db,
    PlatformAuditor auditor,
    PlatformDomainPasswordProtector passwordProtector) : ControllerBase
{
    private const int ListLimit = 1000;

    [HttpGet]
    public async Task<DomainListResult> GetList()
    {
        var rows = await db.PlatformDomains
            .AsNoTracking()
            .OrderBy(item => item.EndDate == null)
            .ThenBy(item => item.EndDate)
            .Select(item => new DomainListItemDto(
                item.Id,
                item.DomainName,
                item.Registrar,
                item.StartDate,
                item.EndDate,
                db.PlatformWebsites.Count(site => site.FK_PlatformDomainId == item.Id)))
            .Take(ListLimit + 1)   // 多拿一筆，用來判斷是否被截斷
            .ToListAsync(HttpContext.RequestAborted);

        return new DomainListResult(rows.Take(ListLimit).ToList(), rows.Count > ListLimit);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<DomainDetailDto>> GetDetail(long id)
    {
        var detail = await LoadDetailAsync(id);
        return detail is null ? NotFound() : detail;
    }

    /// <summary>網站編輯頁輸入網址後呼叫。一律回 200，由 Host／Domain 是否為 null 判斷結果。</summary>
    [HttpGet("match")]
    public async Task<DomainMatchDto> Match([FromQuery] string? url)
    {
        var host = PlatformDomainName.ToHost(url);
        if (host is null)
            return new DomainMatchDto(null, null, null);

        var domain = await db.PlatformDomains
            .AsNoTracking()
            .FindBestMatchAsync(host, HttpContext.RequestAborted);

        return new DomainMatchDto(
            host,
            PlatformDomainName.Suggest(host),
            domain is null
                ? null
                : new DomainSummaryDto(domain.Id, domain.DomainName, domain.Registrar, domain.EndDate));
    }

    [HttpPost]
    public async Task<ActionResult<DomainDetailDto>> Create(DomainSaveRequest request)
    {
        var domainName = await ValidateRequestAsync(request, current: null);
        if (!ModelState.IsValid || domainName is null)
            return ValidationProblem(ModelState);

        var domain = new PlatformDomain();
        Apply(request, domainName, domain);

        db.PlatformDomains.Add(domain);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);

        var detail = await LoadDetailAsync(domain.Id);
        return detail is null ? NotFound() : detail;
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<DomainDetailDto>> Update(long id, DomainSaveRequest request)
    {
        var domain = await db.PlatformDomains
            .FirstOrDefaultAsync(item => item.Id == id, HttpContext.RequestAborted);
        if (domain is null)
            return NotFound();

        var domainName = await ValidateRequestAsync(request, domain);
        if (!ModelState.IsValid || domainName is null)
            return ValidationProblem(ModelState);

        Apply(request, domainName, domain);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);

        var detail = await LoadDetailAsync(domain.Id);
        return detail is null ? NotFound() : detail;
    }

    /// <summary>
    /// 「眼睛」按鈕專用。目前是純讀取（暫不寫稽核），所以用 GET。
    /// ⚠ 日後若要補「誰看過」的稽核寫入，必須改成 POST 走防偽 token。
    /// </summary>
    [HttpGet("{id:long}/password")]
    public async Task<ActionResult<DomainPasswordDto>> GetPassword(long id)
    {
        var domain = await db.PlatformDomains
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new { item.PasswordCipher })
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (domain is null)
            return NotFound();

        // 解不開也回 200 + State=Unreadable，前端顯示提示而不是整頁失敗
        var result = passwordProtector.Unprotect(domain.PasswordCipher);
        return new DomainPasswordDto(result.State, result.Password);
    }

    private Task<DomainDetailDto?> LoadDetailAsync(long id)
    {
        return db.PlatformDomains
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new DomainDetailDto(
                item.Id,
                item.DomainName,
                item.Registrar,
                item.StartDate,
                item.EndDate,
                item.PasswordCipher != null,
                item.Remark,
                db.PlatformWebsites.Count(site => site.FK_PlatformDomainId == item.Id)))
            .FirstOrDefaultAsync(HttpContext.RequestAborted);
    }

    /// <summary>回傳正規化後的網域；格式錯誤回 null 並寫入 ModelState。</summary>
    private async Task<string?> ValidateRequestAsync(DomainSaveRequest request, PlatformDomain? current)
    {
        // 使用者可能貼整串網址，一律轉成小寫主機名稱再存，比對時才對得上
        var domainName = PlatformDomainName.ToHost(request.DomainName);
        var currentId = current?.Id ?? 0;

        if (domainName is null)
            ModelState.AddModelError(nameof(request.DomainName), "網域格式不正確，例：example.com.tw");
        else if (await db.PlatformDomains.AnyAsync(
                     item => item.DomainName == domainName && item.Id != currentId,
                     HttpContext.RequestAborted))
            ModelState.AddModelError(nameof(request.DomainName), $"網域「{domainName}」已經建立過。");
        else if (current is not null && current.DomainName != domainName &&
                 await db.PlatformWebsites.AnyAsync(
                     site => site.FK_PlatformDomainId == current.Id,
                     HttpContext.RequestAborted))
            // 改名後既有網站的網址就對不上，關聯會變成錯的
            ModelState.AddModelError(nameof(request.DomainName), "已有網站使用此網域，不可修改網域名稱。");

        if (request.StartDate is not null && request.EndDate is not null &&
            request.EndDate < request.StartDate)
            ModelState.AddModelError(nameof(request.EndDate), "網域到期日期不可早於起始日期。");

        if (request.ClearPassword && !string.IsNullOrWhiteSpace(request.Password))
            ModelState.AddModelError(nameof(request.Password), "已勾選清除網域密碼，不可同時輸入新密碼。");

        return domainName;
    }

    private void Apply(DomainSaveRequest request, string domainName, PlatformDomain domain)
    {
        domain.DomainName = domainName;
        domain.Registrar = Clean(request.Registrar);
        domain.StartDate = request.StartDate?.Date;
        domain.EndDate = request.EndDate?.Date;
        domain.Remark = Clean(request.Remark);

        if (request.ClearPassword)
            domain.PasswordCipher = null;
        else if (!string.IsNullOrWhiteSpace(request.Password))
            domain.PasswordCipher = passwordProtector.Protect(request.Password);
        // 兩者皆無 → 保留原密文
    }

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}