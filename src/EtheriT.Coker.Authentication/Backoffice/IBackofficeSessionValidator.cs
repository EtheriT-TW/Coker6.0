namespace EtheriT.Coker.Authentication.Backoffice;

public interface IBackofficeSessionValidator
{
    Task<BackofficeSessionValidationResult> ValidateAndRenewAsync(
        string account,
        Guid sessionId,
        CancellationToken cancellationToken = default);
}
