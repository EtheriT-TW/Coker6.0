using EtheriT.Coker.Core.Models;
using Microsoft.AspNetCore.Authorization;

namespace EtheriT.Coker.Web.Platform.Security;

public sealed class PlatformAccessHandler(PlatformAuthorizationService authorizationService)
    : AuthorizationHandler<PlatformAccessRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PlatformAccessRequirement requirement)
    {
        var canAccess = await authorizationService.HasAnyRoleAsync(
            context.User.Identity?.Name,
            PlatformRoleCodes.All);

        if (canAccess)
        {
            context.Succeed(requirement);
        }
    }
}
