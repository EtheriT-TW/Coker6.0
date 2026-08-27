using System.Security.Cryptography;
using System.Text.Json;
using EtheriT.Coker.Application.Shared.Dto.Remote;
using Microsoft.AspNetCore.DataProtection;

namespace EtheriT.Coker.Web.Public.Services
{
    public sealed class RemoteTrackingTokenService
    {
        private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(2);
        private readonly ITimeLimitedDataProtector protector;

        public RemoteTrackingTokenService(IDataProtectionProvider dataProtectionProvider)
        {
            protector = dataProtectionProvider
                .CreateProtector("Coker.RemoteTracking.Page.v1")
                .ToTimeLimitedDataProtector();
        }

        public string Protect(RemoteInputDto page)
        {
            return protector.Protect(JsonSerializer.Serialize(page), TokenLifetime);
        }

        public bool TryUnprotect(string token, out RemoteInputDto page)
        {
            page = new RemoteInputDto();
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                var json = protector.Unprotect(token);
                var value = JsonSerializer.Deserialize<RemoteInputDto>(json);
                if (value == null || value.FK_WebsiteId <= 0 || value.FK_WebmenuId <= 0)
                    return false;

                page = value;
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
