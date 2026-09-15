namespace EtheriT.Coker.Authentication.Backoffice;

public readonly record struct BackofficeSessionValidationResult(
    bool IsValid,
    bool WasRenewed,
    DateTimeOffset? ExpiresAt)
{
    public static BackofficeSessionValidationResult Invalid => new(false, false, null);
}
