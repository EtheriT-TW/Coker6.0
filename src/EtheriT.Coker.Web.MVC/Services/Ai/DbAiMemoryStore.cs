using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using EtheriT.Coker.Application.Shared.Ai;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.MVC.Services.Ai;

public sealed class DbAiMemoryStore(IServiceScopeFactory scopes) : IAiMemoryStore
{
    private static IQueryable<AiMemoryEntry> Query(CokerDbContext db, AiMemoryScope scope, string key) => db.AiMemoryEntries.Where(e =>
        e.ScopeKey == scope.Key && e.MemoryKey == key && e.ScopeKind == scope.Kind && e.WebsiteId == scope.WebsiteId && e.UserId == scope.UserId &&
        e.Feature == scope.Feature && e.Category == scope.Category);

    public async Task<AiMemoryDocument?> ReadAsync(AiMemoryScope scope, string key, CancellationToken token = default)
    {
        Validate(scope, key);
        await using var isolated = scopes.CreateAsyncScope();
        var db = isolated.ServiceProvider.GetRequiredService<CokerDbContext>();
        return await Query(db, scope, key).AsNoTracking().Select(e => new AiMemoryDocument(e.Payload, e.ConcurrencyToken, e.Version)).SingleOrDefaultAsync(token);
    }

    public async Task<AiMemoryWriteResult> WriteAsync(AiMemoryScope scope, string key, string payload, Guid? expectedToken, long? actorUserId, CancellationToken token = default)
    {
        Validate(scope, key);
        if (Encoding.UTF8.GetByteCount(payload) > 3_000_000) throw new ArgumentException("AI memory payload is too large.");
        using var document = JsonDocument.Parse(payload);
        if (document.RootElement.ValueKind != JsonValueKind.Object) throw new ArgumentException("AI memory payload must be an object.");
        if (actorUserId is <= 0) throw new ArgumentException("Invalid memory actor.");
        await using var isolated = scopes.CreateAsyncScope();
        var db = isolated.ServiceProvider.GetRequiredService<CokerDbContext>();
        var current = await Query(db, scope, key).SingleOrDefaultAsync(token);
        if ((current == null && expectedToken != null) || (current != null && current.ConcurrencyToken != expectedToken)) return new(false, null, 0);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
        if (current != null && current.ContentHash == hash) return new(true, current.ConcurrencyToken, current.Version);
        var now = DateTime.UtcNow;
        if (current == null)
        {
            current = new() { ScopeKind = scope.Kind, WebsiteId = scope.WebsiteId, UserId = scope.UserId, Feature = scope.Feature,
                Category = scope.Category, ScopeKey = scope.Key, MemoryKey = key, CreatedAtUtc = now };
            db.AiMemoryEntries.Add(current);
        }
        var root = document.RootElement;
        string? Fingerprint(string name) => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String &&
            Regex.IsMatch(value.GetString()!, @"\A[a-fA-F0-9]{64}\z") ? value.GetString() : null;
        current.Payload = payload;
        current.ContentHash = hash;
        current.SourceHash = Fingerprint("SourceHash");
        current.RuntimeHash = Fingerprint("RuntimeHash");
        current.SchemaVersion = root.TryGetProperty("Schema", out var schema) && schema.ValueKind == JsonValueKind.Number && schema.TryGetInt32(out var version) ? version : 1;
        current.State = scope.Category == "reply-locale" ? "preference" : "source-analysis";
        current.Version = checked(current.Version + 1);
        current.ConcurrencyToken = Guid.NewGuid();
        current.UpdatedAtUtc = now;
        db.AiMemoryRevisions.Add(new() { Entry = current, Version = current.Version, Payload = payload, ContentHash = hash,
            SourceHash = current.SourceHash, RuntimeHash = current.RuntimeHash, SchemaVersion = current.SchemaVersion,
            State = current.State, ActorUserId = actorUserId, RecordedAtUtc = now });
        try
        {
            // One SaveChanges: entry and history participate in the same EF SQL transaction.
            // The dedicated context cannot accidentally save a caller's pending business edits.
            await db.SaveChangesAsync(token);
            return new(true, current.ConcurrencyToken, current.Version);
        }
        catch (DbUpdateConcurrencyException) { return new(false, null, 0); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number is 2601 or 2627)
        { return new(false, null, 0); }
    }

    private static void Validate(AiMemoryScope scope, string key)
    {
        scope.Validate();
        if (!Regex.IsMatch(key, @"\A[a-f0-9]{64}\z")) throw new ArgumentException("Invalid AI memory key.");
    }
}
