using System.Data;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Authentication.Backoffice;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.SystemAdministrators;
using EtheriT.Coker.Web.Platform.Services;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/system-administrators")]
[Microsoft.AspNetCore.Authorization.Authorize(Policy = BackofficeAuthorizationPolicies.PlatformAdministration)]
public sealed class SystemAdministratorsController(
    CokerDbContext db,
    PlatformAuditor auditor,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) : ControllerBase
{
    private const int UserLimit = 2000;
    private static readonly PasswordHasher<User> PasswordHasher = new();

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
            .Where(user =>
                !user.IsDeleted &&
                user.Account != null &&
                user.Account != string.Empty &&
                db.MappingUserAndRoles.Any(mapping =>
                    !mapping.IsDeleted &&
                    mapping.UserId == user.Id &&
                    mapping.Role != null &&
                    !mapping.Role.IsDeleted &&
                    mapping.Role.Type == RoleTypeEnum.系統維護))
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

        var mvcRoles = await db.Roles.AsNoTracking()
            .Where(role => !role.IsDeleted && role.Type == RoleTypeEnum.系統維護)
            .OrderBy(role => role.Ser_No)
            .ThenBy(role => role.Name)
            .Select(role => new SystemAdministratorMvcRoleOptionDto(role.Id, role.Name))
            .ToListAsync(HttpContext.RequestAborted);

        var invitations = await db.PlatformAdministratorInvitations.AsNoTracking()
            .Where(invitation =>
                !invitation.IsDeleted &&
                invitation.ApprovedAtUtc == null &&
                invitation.RevokedAtUtc == null &&
                invitation.User != null &&
                !invitation.User.IsDeleted &&
                (invitation.MvcRoleId == null ||
                    (invitation.MvcRole != null && !invitation.MvcRole.IsDeleted)) &&
                (invitation.PlatformRoleId == null ||
                    (invitation.PlatformRole != null && !invitation.PlatformRole.IsDeleted)))
            .OrderByDescending(invitation => invitation.Id)
            .Select(invitation => new SystemAdministratorInvitationDto(
                invitation.Id,
                invitation.UserId,
                invitation.User!.Name ?? string.Empty,
                invitation.InvitedEmail,
                invitation.User.Account,
                invitation.MvcRoleId,
                invitation.MvcRole == null ? null : invitation.MvcRole.Name,
                invitation.PlatformRoleId,
                invitation.PlatformRole == null ? null : invitation.PlatformRole.Name,
                invitation.ExpiresAtUtc,
                invitation.EmailVerifiedAtUtc))
            .ToListAsync(HttpContext.RequestAborted);

        var mvcMappings = await db.MappingUserAndRoles.AsNoTracking()
            .Where(mapping => !mapping.IsDeleted &&
                mapping.User != null && !mapping.User.IsDeleted &&
                mapping.Role != null && !mapping.Role.IsDeleted &&
                mapping.Role.Type == RoleTypeEnum.系統維護)
            .Select(mapping => new
            {
                mapping.UserId,
                mapping.User!.Name,
                mapping.User.Account,
                mapping.User.Email,
                RoleName = mapping.Role!.Name
            })
            .ToListAsync(HttpContext.RequestAborted);
        var mvcAdministrators = mvcMappings
            .GroupBy(mapping => new { mapping.UserId, mapping.Name, mapping.Account, mapping.Email })
            .OrderBy(group => group.Key.Name)
            .ThenBy(group => group.Key.Account)
            .Select(group => new MvcSystemAdministratorDto(
                group.Key.UserId,
                group.Key.Name ?? string.Empty,
                group.Key.Account ?? string.Empty,
                group.Key.Email,
                string.Join("、", group.Select(mapping => mapping.RoleName).Distinct().OrderBy(name => name)),
                group.Key.UserId == currentUserId))
            .ToList();

        return new SystemAdministratorPageDto(administrators, users, roles, mvcRoles, invitations, mvcAdministrators);
    }

    [HttpPost("users")]
    public async Task<ActionResult<CreateSystemAdministratorInvitationResponse>> CreateUser(
        CreateSystemAdministratorRequest request)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        var email = request.Email?.Trim() ?? string.Empty;

        if (name.Length is < 1 or > 150)
            ModelState.AddModelError(nameof(request.Name), "姓名為必填，且不可超過 150 個字元。");
        if (email.Length is < 3 or > 150 || !new EmailAddressAttribute().IsValid(email))
            ModelState.AddModelError(nameof(request.Email), "請輸入正確的 Email，且不可超過 150 個字元。");
        if (request.MvcRoleId is null && request.PlatformRoleId is null)
            ModelState.AddModelError(nameof(request.MvcRoleId), "請至少選擇一個 MVC 或平台角色。");
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        try
        {
            var executionStrategy = db.Database.CreateExecutionStrategy();
            var invitation = await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    HttpContext.RequestAborted);

                if (await db.Users.AnyAsync(user =>
                    !user.IsDeleted &&
                    user.Email != null && user.Email == email,
                    HttpContext.RequestAborted))
                {
                    throw new InvitationConflictException("此 Email 已有使用者帳號；請從既有 MVC 系統管理者清單分配角色。");
                }

                Role? mvcRole = null;
                if (request.MvcRoleId.HasValue)
                {
                    mvcRole = await db.Roles.FirstOrDefaultAsync(role =>
                        role.Id == request.MvcRoleId.Value && !role.IsDeleted &&
                        role.Type == RoleTypeEnum.系統維護,
                        HttpContext.RequestAborted);
                    if (mvcRole is null)
                        throw new InvitationValidationException(nameof(request.MvcRoleId), "找不到指定的 MVC 系統角色。");
                }

                PlatformRole? platformRole = null;
                if (request.PlatformRoleId.HasValue)
                {
                    platformRole = await db.PlatformRoles.FirstOrDefaultAsync(role =>
                        role.Id == request.PlatformRoleId.Value && !role.IsDeleted && role.IsEnabled,
                        HttpContext.RequestAborted);
                    if (platformRole is null)
                        throw new InvitationValidationException(nameof(request.PlatformRoleId), "找不到指定的客戶管理平台角色。");
                }

                var user = new User
                {
                    Name = name,
                    Account = null,
                    Email = email,
                    Password = string.Empty
                };
                user.Password = PasswordHasher.HashPassword(user, Guid.NewGuid().ToString("N"));
                var pendingInvitation = new PlatformAdministratorInvitation
                {
                    User = user,
                    MvcRole = mvcRole,
                    PlatformRole = platformRole,
                    InvitedEmail = email,
                    ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
                };
                db.Users.Add(user);
                db.PlatformAdministratorInvitations.Add(pendingInvitation);

                await auditor.SaveChangesAsync(HttpContext.RequestAborted);
                await transaction.CommitAsync(HttpContext.RequestAborted);

                return new SystemAdministratorInvitationDto(
                    pendingInvitation.Id,
                    user.Id,
                    user.Name,
                    email,
                    null,
                    mvcRole?.Id,
                    mvcRole?.Name,
                    platformRole?.Id,
                    platformRole?.Name,
                    pendingInvitation.ExpiresAtUtc,
                    null);
            });

            var emailError = await SendInvitationEmailAsync(email, HttpContext.RequestAborted);
            return Ok(new CreateSystemAdministratorInvitationResponse(
                invitation,
                emailError is null,
                emailError));
        }
        catch (InvitationConflictException exception)
        {
            return Conflict(new { Message = exception.Message });
        }
        catch (InvitationValidationException exception)
        {
            ModelState.AddModelError(exception.Field, exception.Message);
            return ValidationProblem(ModelState);
        }
    }

    [HttpPost("invitations/{invitationId:long}/resend")]
    public async Task<IActionResult> ResendInvitation(long invitationId)
    {
        var invitation = await db.PlatformAdministratorInvitations
            .Include(item => item.User)
            .FirstOrDefaultAsync(item =>
                item.Id == invitationId &&
                !item.IsDeleted &&
                item.ApprovedAtUtc == null &&
                item.RevokedAtUtc == null,
                HttpContext.RequestAborted);
        if (invitation?.User is null || invitation.User.IsDeleted)
            return NotFound();
        if (invitation.EmailVerifiedAtUtc is not null)
            return Conflict(new { Message = "此 Email 已完成驗證，請直接進行管理員核准。" });

        invitation.ExpiresAtUtc = DateTime.UtcNow.AddDays(7);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        var emailError = await SendInvitationEmailAsync(invitation.InvitedEmail, HttpContext.RequestAborted);
        return emailError is null
            ? NoContent()
            : StatusCode(StatusCodes.Status502BadGateway, new { Message = emailError });
    }

    [HttpPost("invitations/{invitationId:long}/approve")]
    public async Task<IActionResult> ApproveInvitation(long invitationId)
    {
        var executionStrategy = db.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                HttpContext.RequestAborted);
            var invitation = await db.PlatformAdministratorInvitations
                .Include(item => item.User)
                .Include(item => item.MvcRole)
                .Include(item => item.PlatformRole)
                .FirstOrDefaultAsync(item =>
                    item.Id == invitationId &&
                    !item.IsDeleted &&
                    item.ApprovedAtUtc == null &&
                    item.RevokedAtUtc == null,
                    HttpContext.RequestAborted);
            if (invitation?.User is null || invitation.User.IsDeleted ||
                (invitation.MvcRoleId.HasValue && (invitation.MvcRole is null || invitation.MvcRole.IsDeleted)) ||
                (invitation.PlatformRoleId.HasValue &&
                    (invitation.PlatformRole is null || invitation.PlatformRole.IsDeleted || !invitation.PlatformRole.IsEnabled)))
                return NotFound();
            if (invitation.EmailVerifiedAtUtc is null || string.IsNullOrWhiteSpace(invitation.User.Account))
                return Conflict(new { Message = "受邀者尚未完成 Email 驗證與帳號設定。" });

            if (invitation.MvcRoleId is null && invitation.PlatformRoleId is null)
                return Conflict(new { Message = "邀請未指定任何管理角色。" });

            if (invitation.MvcRoleId.HasValue)
            {
                db.MappingUserAndRoles.Add(new MappingUserAndRole
                {
                    UserId = invitation.UserId,
                    RoleId = invitation.MvcRoleId.Value
                });
            }
            if (invitation.PlatformRoleId.HasValue)
            {
                db.MappingUserAndPlatformRoles.Add(new MappingUserAndPlatformRole
                {
                    UserId = invitation.UserId,
                    PlatformRoleId = invitation.PlatformRoleId.Value
                });
            }
            invitation.ApprovedAtUtc = DateTime.UtcNow;
            invitation.ApprovedByUserId = await auditor.GetCurrentUserIdAsync(HttpContext.RequestAborted);

            await auditor.SaveChangesAsync(HttpContext.RequestAborted);
            await transaction.CommitAsync(HttpContext.RequestAborted);
            return NoContent();
        });
    }

    [HttpDelete("invitations/{invitationId:long}")]
    public async Task<IActionResult> RevokeInvitation(long invitationId)
    {
        var invitation = await db.PlatformAdministratorInvitations
            .Include(item => item.User)
            .FirstOrDefaultAsync(item =>
                item.Id == invitationId &&
                !item.IsDeleted &&
                item.ApprovedAtUtc == null &&
                item.RevokedAtUtc == null,
                HttpContext.RequestAborted);
        if (invitation?.User is null || invitation.User.IsDeleted)
            return NotFound();

        var currentUserId = await auditor.GetCurrentUserIdAsync(HttpContext.RequestAborted);
        invitation.RevokedAtUtc = DateTime.UtcNow;
        invitation.RevokedByUserId = currentUserId;
        invitation.User.ForgetID = null;
        invitation.User.ForgeIDSendDate = null;
        db.Users.Remove(invitation.User);
        await auditor.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<SystemAdministratorDto>> Add(AddSystemAdministratorRequest request)
    {
        var executionStrategy = db.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync<ActionResult<SystemAdministratorDto>>(async () =>
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
        });
    }

    [HttpDelete("users/{userId:long}/mvc-roles")]
    public async Task<IActionResult> RemoveMvcAdministrator(long userId)
    {
        var executionStrategy = db.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync<IActionResult>(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                HttpContext.RequestAborted);

            var currentUserId = await auditor.GetCurrentUserIdAsync(HttpContext.RequestAborted);
            if (userId == currentUserId)
                return Conflict(new { Message = "不可取消自己的 MVC 系統管理者身分。" });

            var activeMappings = db.MappingUserAndRoles
                .Where(mapping => !mapping.IsDeleted &&
                    mapping.User != null && !mapping.User.IsDeleted &&
                    mapping.Role != null && !mapping.Role.IsDeleted &&
                    mapping.Role.Type == RoleTypeEnum.系統維護);
            var mappings = await activeMappings.Where(mapping => mapping.UserId == userId)
                .ToListAsync(HttpContext.RequestAborted);
            if (mappings.Count == 0)
                return NotFound();

            if (!await activeMappings.AnyAsync(mapping => mapping.UserId != userId,
                HttpContext.RequestAborted))
                return Conflict(new { Message = "至少必須保留一位 MVC 系統管理者。" });

            db.MappingUserAndRoles.RemoveRange(mappings);
            await auditor.SaveChangesAsync(HttpContext.RequestAborted);
            await transaction.CommitAsync(HttpContext.RequestAborted);
            return NoContent();
        });
    }

    [HttpDelete("{mappingId:long}")]
    public async Task<IActionResult> Remove(long mappingId)
    {
        var executionStrategy = db.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync<IActionResult>(async () =>
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
        });
    }

    private async Task<string?> SendInvitationEmailAsync(string email, CancellationToken cancellationToken)
    {
        var mvcUrl = configuration["SystemLinks:MvcUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(mvcUrl))
            return "邀請已建立，但未設定 MVC 網址，無法寄出啟用信。";

        try
        {
            var client = httpClientFactory.CreateClient();
            using var response = await client.PostAsJsonAsync(
                $"{mvcUrl}/api/User/RequestPasswordReset",
                new { Email = email },
                cancellationToken);
            if (!response.IsSuccessStatusCode)
                return $"邀請已建立，但 MVC 寄信服務回傳 HTTP {(int)response.StatusCode}。";

            var result = await response.Content.ReadFromJsonAsync<MvcMailResponse>(
                cancellationToken: cancellationToken);
            return result?.Success == true
                ? null
                : result?.Error ?? result?.Message ?? "邀請已建立，但啟用信寄送失敗。";
        }
        catch (Exception exception)
        {
            return $"邀請已建立，但無法連線至 MVC 寄信服務：{exception.Message}";
        }
    }

    private sealed record MvcMailResponse(bool Success, string? Message, string? Error);
    private sealed class InvitationConflictException(string message) : Exception(message);
    private sealed class InvitationValidationException(string field, string message) : Exception(message)
    {
        public string Field { get; } = field;
    }
}
