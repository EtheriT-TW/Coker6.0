using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Security;

public sealed class PlatformAccessHandler(CokerDbContext db)
    : AuthorizationHandler<PlatformAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PlatformAccessRequirement requirement)
    {
        var account = context.User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(account))
        {
            return;
        }

        var canAccess = await db.MappingUserAndRoles
            .AsNoTracking()
            .AnyAsync(mapping =>
                !mapping.IsDeleted &&
                mapping.User != null &&
                !mapping.User.IsDeleted &&
                mapping.User.Account == account &&
                mapping.Role != null &&
                !mapping.Role.IsDeleted &&
                mapping.Role.Type == RoleTypeEnum.系統維護);

        if (canAccess)
        {
            context.Succeed(requirement);
        }
    }
}
