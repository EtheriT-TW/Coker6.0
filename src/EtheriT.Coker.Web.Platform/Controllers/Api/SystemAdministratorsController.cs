using System.Data;
using EtheriT.Coker.Authentication.Backoffice;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.SystemAdministrators;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/system-administrators")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = BackofficeAuthorizationPolicies.PlatformAdministration)]
public sealed class SystemAdministratorsController(CokerDbContext db, PlatformAuditor auditor) : ControllerBase
{
    private const int UserLimit = 2000;

    [HttpGet]
    public async Task<SystemAdministratorPageDto> GetPage()
    {
        var currentUserId = await auditor.GetCurrentUserIdAsync(HttpContext.RequestAborted);
        var administrators = await (
            from mapping in db.MappingUserAndPlatformRoles.AsNoTracking()
            join user in db.Users.AsNoTracking() on mapping.UserId equals user.Id
            join role in db.PlatformRoles.AsNoTracking() on mapping.PlatformRoleId equals role.Id
            where !mapping.IsDeleted && !user.IsDeleted && !role.IsDeleted &&
                  role.IsEnabled
            orderby user.Name, user.Account, role.Sort, role.Name
            select new SystemAdministratorDto(
                mapping.Id,
                user.Id,
                user.Name ?? string.Empty,
                user.Account ?? string.Empty,
                user.Email,
                role.Id,
                role.Code,
                role.Name ?? string.Empty,
                user.Id == currentUserId))
            .ToListAsync(HttpContext.RequestAborted);

        var users = await db.Users.AsNoTracking()
            .Where(user => !user.IsDeleted && user.Account != null && user.Account != string.Empty)
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Account)
            .Take(UserLimit)
            .Select(user => new SystemAdministratorUserOptionDto(
                user.Id,
                user.Name ?? string.Empty,
                user.Account!,
                user.Email))
            .ToListAsync(HttpContext.RequestAborted);

        var roles = await db.PlatformRoles.AsNoTracking()
            .Where(role => !role.IsDeleted && role.IsEnabled)
            .OrderBy(role => role.Sort)
            .ThenBy(role => role.Name)
            .Select(role => new SystemAdministratorRoleOptionDto(
                role.Id,
                role.Code,
                role.Name,
                role.Description))
            .ToListAsync(HttpContext.RequestAborted);

        return new SystemAdministratorPageDto(administrators, users, roles);
    }

    [HttpPost]
    public async Task<ActionResult<SystemAdministratorDto>> Add(AddSystemAdministratorRequest request)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            HttpContext.RequestAborted);

        var user = await db.Users
            .FirstOrDefaultAsync(item => item.Id == request.UserId && !item.IsDeleted, HttpContext.RequestAborted);
        if (user is null)
        {
            ModelState.AddModelError(nameof(request.UserId), "找不到指定的使用者。");
            return ValidationProblem(ModelState);
        }

        var role = await db.PlatformRoles
            .FirstOrDefaultAsync(item => item.Id == request.RoleId && !item.IsDeleted && item.IsEnabled,
                HttpContext.RequestAborted);
        if (role is null)
        {
            ModelState.AddModelError(nameof(request.RoleId), "找不到指定的 Platform 角色。");
            return ValidationProblem(ModelState);
        }

        var hasPlatformAdministrator = await db.MappingUserAndPlatformRoles
            .AnyAsync(item =>
                !item.IsDeleted &&
                item.User != null && !item.User.IsDeleted &&
                item.PlatformRole != null && !item.PlatformRole.IsDeleted && item.PlatformRole.IsEnabled &&
                item.PlatformRole.Code == PlatformRoleCodes.Administrator,
                HttpContext.RequestAborted);
        if (!hasPlatformAdministrator && role.Code != PlatformRoleCodes.Administrator)
        {
            return Conflict(new
            {
                Message = "首次分配 Platform 角色必須先建立一位 Platform 總管理者。"
            });
        }

        if (await db.MappingUserAndPlatformRoles.AnyAsync(item =>
                !item.IsDeleted && item.UserId == request.UserId && item.PlatformRoleId == request.RoleId,
                HttpContext.RequestAborted))
        {
            return Conflict(new { Message = "此使用者已經具有指定的 Platform 角色。" });
        }

        var mapping = await db.MappingUserAndPlatformRoles
            .IgnoreQueryFilters()
            .Where(item => item.IsDeleted && item.UserId == request.UserId && item.PlatformRoleId == request.RoleId)
            .OrderByDescending(item => item.Id)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (mapping is null)
        {
            mapping = new MappingUserAndPlatformRole
            {
                UserId = user.Id,
                PlatformRoleId = role.Id
            };
            db.MappingUserAndPlatformRoles.Add(mapping);
        }
        else
        {
            mapping.IsDeleted = false;
            mapping.DeleterUserId = null;
            mapping.DeletionTime = null;
        }

        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);
        var currentUserId = await auditor.GetCurrentUserIdAsync(HttpContext.RequestAborted);
        return Ok(new SystemAdministratorDto(
            mapping.Id,
            user.Id,
            user.Name ?? string.Empty,
            user.Account ?? string.Empty,
            user.Email,
            role.Id,
            role.Code,
            role.Name ?? string.Empty,
            user.Id == currentUserId));
    }

    [HttpDelete("{mappingId:long}")]
    public async Task<IActionResult> Remove(long mappingId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            HttpContext.RequestAborted);

        var mapping = await db.MappingUserAndPlatformRoles
            .Include(item => item.User)
            .Include(item => item.PlatformRole)
            .FirstOrDefaultAsync(item => item.Id == mappingId && !item.IsDeleted,
                HttpContext.RequestAborted);
        if (mapping is null || mapping.User is null || mapping.User.IsDeleted ||
            mapping.PlatformRole is null || mapping.PlatformRole.IsDeleted)
        {
            return NotFound();
        }

        var currentUserId = await auditor.GetCurrentUserIdAsync(HttpContext.RequestAborted);
        if (mapping.UserId == currentUserId)
            return Conflict(new { Message = "不可移除自己目前的 Platform 角色。" });

        if (mapping.PlatformRole.Code == PlatformRoleCodes.Administrator)
        {
            var administratorCount = await db.MappingUserAndPlatformRoles
                .Where(item => !item.IsDeleted && item.User != null && !item.User.IsDeleted &&
                    item.PlatformRole != null && !item.PlatformRole.IsDeleted && item.PlatformRole.IsEnabled &&
                    item.PlatformRole.Code == PlatformRoleCodes.Administrator)
                .Select(item => item.UserId)
                .Distinct()
                .CountAsync(HttpContext.RequestAborted);
            if (administratorCount <= 1)
                return Conflict(new { Message = "Platform 至少必須保留一位總管理者。" });
        }

        db.MappingUserAndPlatformRoles.Remove(mapping);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
