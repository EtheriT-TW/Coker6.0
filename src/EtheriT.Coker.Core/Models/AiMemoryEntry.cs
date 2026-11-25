namespace EtheriT.Coker.Core.Models;

// A current value plus immutable revision history. Authorization is resolved by the
// calling feature before constructing a scope; these rows never grant permissions.
public sealed class AiMemoryEntry
{
    public long Id { get; set; }
    public byte ScopeKind { get; set; } // 0 system, 1 website, 2 person
    public long? WebsiteId { get; set; }
    public long? UserId { get; set; }
    public string Feature { get; set; } = "";
    public string Category { get; set; } = "";
    public string ScopeKey { get; set; } = "";
    public string MemoryKey { get; set; } = "";
    public string Payload { get; set; } = "";
    public string ContentHash { get; set; } = "";
    public string? SourceHash { get; set; }
    public string? RuntimeHash { get; set; }
    public int SchemaVersion { get; set; }
    public string State { get; set; } = "";
    public long Version { get; set; }
    public Guid ConcurrencyToken { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class AiMemoryRevision
{
    public long Id { get; set; }
    public long EntryId { get; set; }
    public AiMemoryEntry Entry { get; set; } = null!;
    public long Version { get; set; }
    public string Payload { get; set; } = "";
    public string ContentHash { get; set; } = "";
    public string? SourceHash { get; set; }
    public string? RuntimeHash { get; set; }
    public int SchemaVersion { get; set; }
    public string State { get; set; } = "";
    public long? ActorUserId { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}
