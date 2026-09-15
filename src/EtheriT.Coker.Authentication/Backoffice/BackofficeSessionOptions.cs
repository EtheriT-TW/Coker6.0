namespace EtheriT.Coker.Authentication.Backoffice;

public sealed class BackofficeSessionOptions
{
    public const string SectionName = "BackofficeAuthentication:Session";

    public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes(30);

    public TimeSpan RenewalThreshold { get; set; } = TimeSpan.FromMinutes(15);

    public TimeSpan ActivityPingInterval { get; set; } = TimeSpan.FromMinutes(5);
}
