using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Security;

public sealed class PlatformAuthorizationService(CokerDbContext db)
{
    public async Task<IReadOnlySet<string>> GetRoleCodesAsync(
        string? account,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(account))
            return new HashSet<string>(StringComparer.Ordinal);

        var roleCodes = await db.MappingUserAndPlatformRoles
            .AsNoTracking()
            .Where(mapping =>
                !mapping.IsDeleted &&
                mapping.User != null &&
                !mapping.User.IsDeleted &&
                mapping.User.Account == account &&
                mapping.PlatformRole != null &&
                !mapping.PlatformRole.IsDeleted &&
                mapping.PlatformRole.IsEnabled)
            .Select(mapping => mapping.PlatformRole!.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return roleCodes.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<bool> HasAnyRoleAsync(
        string? account,
        IEnumerable<string> acceptedRoleCodes,
        CancellationToken cancellationToken = default)
    {
        var roleCodes = await GetRoleCodesAsync(account, cancellationToken);
        return acceptedRoleCodes.Any(roleCodes.Contains);
    }
}
