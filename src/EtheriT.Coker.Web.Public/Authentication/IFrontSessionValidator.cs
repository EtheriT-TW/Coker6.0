using System.Security.Claims;

namespace EtheriT.Coker.Web.Public.Authentication
{
    public interface IFrontSessionValidator
    {
        Task<FrontSessionValidationResult> ValidateAsync(
            ClaimsPrincipal? principal,
            long websiteId,
            CancellationToken cancellationToken = default);
    }
}
