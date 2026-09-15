using Microsoft.AspNetCore.DataProtection;

namespace EtheriT.Coker.Web.Platform.Security;

public sealed class PlatformReauthenticationTicketService
{
    private readonly ITimeLimitedDataProtector protector;

    public PlatformReauthenticationTicketService(IDataProtectionProvider dataProtectionProvider)
    {
        protector = dataProtectionProvider
            .CreateProtector("EtheriT.Coker.Platform.Reauthentication.v1")
            .ToTimeLimitedDataProtector();
    }

    public string Create(string account)
    {
        return protector.Protect(account, TimeSpan.FromDays(1));
    }

    public bool TryRead(string ticket, out string account)
    {
        try
        {
            account = protector.Unprotect(ticket, out _);
            return !string.IsNullOrWhiteSpace(account);
        }
        catch
        {
            account = string.Empty;
            return false;
        }
    }
}
