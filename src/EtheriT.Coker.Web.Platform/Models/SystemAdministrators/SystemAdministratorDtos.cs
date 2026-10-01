namespace EtheriT.Coker.Web.Platform.Models.SystemAdministrators;

public sealed record SystemAdministratorDto(
    long MappingId,
    long UserId,
    string Name,
    string Account,
    string? Email,
    long RoleId,
    string RoleCode,
    string RoleName,
    bool IsCurrentUser);

public sealed record SystemAdministratorUserOptionDto(long Id, string Name, string Account, string? Email);
public sealed record SystemAdministratorRoleOptionDto(
    long Id,
    string Code,
    string Name,
    string Description);
public sealed record SystemAdministratorMvcRoleOptionDto(long Id, string Name);

public sealed record SystemAdministratorInvitationDto(
    long Id,
    long UserId,
    string Name,
    string Email,
    string? Account,
    long MvcRoleId,
    string MvcRoleName,
    long PlatformRoleId,
    string PlatformRoleName,
    DateTime ExpiresAtUtc,
    DateTime? EmailVerifiedAtUtc);

public sealed record SystemAdministratorPageDto(
    IReadOnlyList<SystemAdministratorDto> Administrators,
    IReadOnlyList<SystemAdministratorUserOptionDto> Users,
    IReadOnlyList<SystemAdministratorRoleOptionDto> Roles,
    IReadOnlyList<SystemAdministratorMvcRoleOptionDto> MvcRoles,
    IReadOnlyList<SystemAdministratorInvitationDto> Invitations);

public sealed record AddSystemAdministratorRequest(long UserId, long RoleId);

public sealed record CreateSystemAdministratorRequest(
    string Name,
    string Email,
    long MvcRoleId,
    long PlatformRoleId);

public sealed record CreateSystemAdministratorInvitationResponse(
    SystemAdministratorInvitationDto Invitation,
    bool EmailSent,
    string? EmailError);
