using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace EtheriT.Coker.Application.Shared.Ai;

// Constructed by authorized feature services, never accepted from model/client JSON.
public sealed record AiMemoryScope(byte Kind, long? WebsiteId, long? UserId, string Feature, string Category)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Feature) || string.IsNullOrWhiteSpace(Category) || Feature.Length > 200 || Category.Length > 40 ||
            Feature.IndexOfAny(['\r', '\n']) >= 0 || Category.IndexOfAny(['\r', '\n']) >= 0 ||
            !(Kind == 0 && WebsiteId == null && UserId == null || Kind == 1 && WebsiteId > 0 && UserId == null || Kind == 2 && WebsiteId > 0 && UserId > 0))
            throw new ArgumentException("Invalid AI memory scope.");
    }
    public string Key => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
        string.Create(CultureInfo.InvariantCulture, $"{Kind}\n{WebsiteId}\n{UserId}\n{Feature}\n{Category}")))).ToLowerInvariant();
}

public sealed record AiMemoryDocument(string Payload, Guid Token, long Version);
public sealed record AiMemoryWriteResult(bool Applied, Guid? Token, long Version);

public interface IAiMemoryStore
{
    Task<AiMemoryDocument?> ReadAsync(AiMemoryScope scope, string key, CancellationToken token = default);
    // Null expected token means insert-only. Updates must supply the token read earlier.
    Task<AiMemoryWriteResult> WriteAsync(AiMemoryScope scope, string key, string payload, Guid? expectedToken,
        long? actorUserId, CancellationToken token = default);
}
