using System.Data;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.SystemAdministrators;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/system-administrators")]
public sealed class SystemAdministratorsController(CokerDbContext db, PlatformAuditor auditor) : ControllerBase
{
    private const int UserLimit = 2000;

    [HttpGet]
    public async Task<SystemAdministratorPageDto> GetPage()
    {
        var currentUserId = await auditor.GetCurrentUserIdAsync(HttpContext.RequestAborted);
        var administrators = await (
            from mapping in db.MappingUserAndRoles.AsNoTracking()
            join user in db.Users.AsNoTracking() on mapping.UserId equals user.Id
            join role in db.Roles.AsNoTracking() on mapping.RoleId equals role.Id
            where !mapping.IsDeleted && !user.IsDeleted && !role.IsDeleted &&
                  role.Type == RoleTypeEnum.系統維護
            orderby user.Name, user.Account, role.Name
            select new SystemAdministratorDto(
                mapping.Id,
                user.Id,
                user.Name ?? string.Empty,
                user.Account ?? string.Empty,
                user.Email,
                role.Id,
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

        var roles = await db.Roles.AsNoTracking()
            .Where(role => !role.IsDeleted && role.Type == RoleTypeEnum.系統維護)
            .OrderByDescending(role => role.IsSuperUser)
            .ThenBy(role => role.Name)
            .Select(role => new SystemAdministratorRoleOptionDto(
                role.Id,
                role.Name ?? string.Empty,
                role.IsSuperUser))
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

        var role = await db.Roles
            .FirstOrDefaultAsync(item => item.Id == request.RoleId && !item.IsDeleted &&
                item.Type == RoleTypeEnum.系統維護, HttpContext.RequestAborted);
        if (role is null)
        {
            ModelState.AddModelError(nameof(request.RoleId), "找不到指定的系統維護角色。");
            return ValidationProblem(ModelState);
        }

        if (await db.MappingUserAndRoles.AnyAsync(item =>
                !item.IsDeleted && item.UserId == request.UserId && item.RoleId == request.RoleId,
                HttpContext.RequestAborted))
        {
            return Conflict(new { Message = "此使用者已經具有指定的系統管理角色。" });
        }

        var mapping = await db.MappingUserAndRoles
            .Where(item => item.IsDeleted && item.UserId == request.UserId && item.RoleId == request.RoleId)
            .OrderByDescending(item => item.Id)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (mapping is null)
        {
            mapping = new MappingUserAndRole
            {
                UserId = user.Id,
                RoleId = role.Id
            };
            db.MappingUserAndRoles.Add(mapping);
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
            role.Name ?? string.Empty,
            user.Id == currentUserId));
    }

    [HttpDelete("{mappingId:long}")]
    public async Task<IActionResult> Remove(long mappingId)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            HttpContext.RequestAborted);

        var mapping = await db.MappingUserAndRoles
            .Include(item => item.User)
            .Include(item => item.Role)
            .FirstOrDefaultAsync(item => item.Id == mappingId && !item.IsDeleted,
                HttpContext.RequestAborted);
        if (mapping is null || mapping.User is null || mapping.User.IsDeleted ||
            mapping.Role is null || mapping.Role.IsDeleted || mapping.Role.Type != RoleTypeEnum.系統維護)
        {
            return NotFound();
        }

        var currentUserId = await auditor.GetCurrentUserIdAsync(HttpContext.RequestAborted);
        if (mapping.UserId == currentUserId)
            return Conflict(new { Message = "不可移除自己目前的系統管理角色。" });

        var targetHasAnotherSystemRole = await db.MappingUserAndRoles
            .AnyAsync(item => !item.IsDeleted && item.UserId == mapping.UserId && item.Id != mapping.Id &&
                item.Role != null && !item.Role.IsDeleted && item.Role.Type == RoleTypeEnum.系統維護,
                HttpContext.RequestAborted);

        if (!targetHasAnotherSystemRole)
        {
            var administratorCount = await db.MappingUserAndRoles
                .Where(item => !item.IsDeleted && item.User != null && !item.User.IsDeleted &&
                    item.Role != null && !item.Role.IsDeleted && item.Role.Type == RoleTypeEnum.系統維護)
                .Select(item => item.UserId)
                .Distinct()
                .CountAsync(HttpContext.RequestAborted);
            if (administratorCount <= 1)
                return Conflict(new { Message = "系統至少必須保留一位管理者。" });
        }

        db.MappingUserAndRoles.Remove(mapping);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);
        return NoContent();
    }
}
