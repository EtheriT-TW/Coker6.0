using Microsoft.AspNetCore.Authorization;

namespace EtheriT.Coker.Web.Platform.Security;

public sealed class PlatformRoleRequirement(params string[] acceptedRoleCodes) : IAuthorizationRequirement
{
    public IReadOnlyList<string> AcceptedRoleCodes { get; } = acceptedRoleCodes;
}

public sealed class PlatformRoleHandler(PlatformAuthorizationService authorizationService)
    : AuthorizationHandler<PlatformRoleRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PlatformRoleRequirement requirement)
    {
        if (await authorizationService.HasAnyRoleAsync(
                context.User.Identity?.Name,
                requirement.AcceptedRoleCodes))
        {
            context.Succeed(requirement);
        }
    }
}
