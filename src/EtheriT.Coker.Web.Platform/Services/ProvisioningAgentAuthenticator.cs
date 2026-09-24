using System.Security.Cryptography;
using System.Text;

namespace EtheriT.Coker.Web.Platform.Services;

public sealed class ProvisioningAgentAuthenticator(IConfiguration configuration)
{
    public bool IsValid(string? serverId, string? suppliedKey)
    {
        if (string.IsNullOrWhiteSpace(serverId)) return false;
        var expectedKey = configuration[$"Provisioning:AgentApiKeys:{serverId.Trim()}"];
        if (string.IsNullOrWhiteSpace(expectedKey) || string.IsNullOrWhiteSpace(suppliedKey))
            return false;

        var expected = Encoding.UTF8.GetBytes(expectedKey);
        var supplied = Encoding.UTF8.GetBytes(suppliedKey);
        return expected.Length == supplied.Length && CryptographicOperations.FixedTimeEquals(expected, supplied);
    }
}
