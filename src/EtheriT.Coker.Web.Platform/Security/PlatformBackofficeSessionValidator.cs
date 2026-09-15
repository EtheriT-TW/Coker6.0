using EtheriT.Coker.Authentication.Backoffice;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Web.Platform.Security;

public sealed class PlatformBackofficeSessionValidator(
    CokerDbContext db,
    IOptions<BackofficeSessionOptions> sessionOptions)
    : IBackofficeSessionValidator
{
    public async Task<BackofficeSessionValidationResult> ValidateAndRenewAsync(
        string account,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await (
            from token in db.Tokens
            join user in db.Users on token.UserID equals user.Id
            where token.id == sessionId &&
                  !user.IsDeleted &&
                  user.Account == account
            select token)
            .SingleOrDefaultAsync(cancellationToken);

        var now = DateTime.Now;
        if (session?.EndTime == null || session.EndTime <= now)
        {
            return BackofficeSessionValidationResult.Invalid;
        }

        var options = sessionOptions.Value;
        var idleTimeout = options.IdleTimeout > TimeSpan.Zero
            ? options.IdleTimeout
            : TimeSpan.FromMinutes(30);
        var renewalThreshold = options.RenewalThreshold > TimeSpan.Zero
            ? options.RenewalThreshold
            : TimeSpan.FromTicks(idleTimeout.Ticks / 2);

        var wasRenewed = false;
        if (session.EndTime <= now.Add(renewalThreshold))
        {
            session.EndTime = now.Add(idleTimeout);
            await db.SaveChangesAsync(cancellationToken);
            wasRenewed = true;
        }

        return new BackofficeSessionValidationResult(
            true,
            wasRenewed,
            new DateTimeOffset(session.EndTime.Value));
    }
}
