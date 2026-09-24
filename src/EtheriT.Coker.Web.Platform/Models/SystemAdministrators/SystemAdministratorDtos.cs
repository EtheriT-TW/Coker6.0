namespace EtheriT.Coker.Web.Platform.Models.SystemAdministrators;

public sealed record SystemAdministratorDto(
    long MappingId,
    long UserId,
    string Name,
    string Account,
    string? Email,
    long RoleId,
    string RoleName,
    bool IsCurrentUser);

public sealed record SystemAdministratorUserOptionDto(long Id, string Name, string Account, string? Email);
public sealed record SystemAdministratorRoleOptionDto(long Id, string Name, bool IsSuperUser);

public sealed record SystemAdministratorPageDto(
    IReadOnlyList<SystemAdministratorDto> Administrators,
    IReadOnlyList<SystemAdministratorUserOptionDto> Users,
    IReadOnlyList<SystemAdministratorRoleOptionDto> Roles);

public sealed record AddSystemAdministratorRequest(long UserId, long RoleId);
