using EtheriT.Coker.Application.Shared.Ai;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.MVC.Services.Ai;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task AiMemoryStoreChecks(BackofficeSandbox box)
    {
        // This executor runs only against the sandbox's independent InMemory database.
        var store = box.Services.GetRequiredService<IAiMemoryStore>();
        var scope = new AiMemoryScope(2, 901, 91, "canvas", "reply-locale");
        var key = new string('a', 64);
        const string initial = """{"Locale":"zh-TW"}""";
        var created = await store.WriteAsync(scope, key, initial, null, 91);
        Require(created.Applied && created.Version == 1 && created.Token != null, "A new memory must create version one.");
        var restarted = new DbAiMemoryStore(box.Services.GetRequiredService<IServiceScopeFactory>());
        var read = await restarted.ReadAsync(scope, key);
        Require(read?.Payload == initial && read.Token == created.Token, "A new store instance must retrieve database memory.");
        var unchanged = await store.WriteAsync(scope, key, initial, read!.Token, 91);
        Require(unchanged.Applied && unchanged.Version == 1 && unchanged.Token == read.Token, "Identical content must not add history or rotate the version token.");
        foreach (var other in new[] {
            scope with { WebsiteId = 902 }, scope with { UserId = 92 }, scope with { Feature = "other" },
            scope with { Category = "other" }, new AiMemoryScope(1, 901, null, "canvas", "reply-locale"),
            new AiMemoryScope(0, null, null, "canvas", "reply-locale") })
            Require(await store.ReadAsync(other, key) == null, "Website, user, feature, category and scope boundaries must remain isolated.");
        var updated = await store.WriteAsync(scope, key, """{"Locale":"ja-JP"}""", read.Token, 91);
        Require(updated.Applied && updated.Version == 2 && updated.Token != read.Token, "An authorized update must advance the version.");
        Require(!(await store.WriteAsync(scope, key, initial, read.Token, 91)).Applied &&
            !(await store.WriteAsync(scope, key, initial, null, 91)).Applied &&
            !(await store.WriteAsync(scope, new string('b', 64), initial, updated.Token, 91)).Applied,
            "Stale updates, duplicate insertions and updates of missing entries must not overwrite memory.");
        var rejected = false;
        try { await store.WriteAsync(scope with { Kind = 0 }, key, initial, null, 91); }
        catch (ArgumentException) { rejected = true; }
        Require(rejected, "Malformed scope ownership must be rejected before persistence.");
        var sourceScope = new AiMemoryScope(0, null, null, "canvas", "source");
        var sourceKey = new string('c', 64);
        var sourcePayload = "{\"Schema\":2,\"SourceHash\":\"" + new string('d', 64) + "\",\"RuntimeHash\":\"" + new string('e', 64) + "\"}";
        // Deliberately leave an unrelated business change pending in the caller context.
        var website = await box.Db.Websites.FirstAsync();
        var originalTitle = website.Title;
        website.Title = "unsaved-memory-fixture";
        try
        {
            Require((await store.WriteAsync(sourceScope, sourceKey, sourcePayload, null, 91)).Applied,
                "Source fingerprints must be persisted through the same memory store.");
            await using var independent = box.Services.CreateAsyncScope();
            var separateDb = independent.ServiceProvider.GetRequiredService<CokerDbContext>();
            Require(await separateDb.Websites.Where(w => w.Id == website.Id).Select(w => w.Title).SingleAsync() == originalTitle,
                "Saving memory must not flush unrelated pending business changes from the caller's context.");
        }
        finally { website.Title = originalTitle; }
        await using var inspection = box.Services.CreateAsyncScope();
        var db = inspection.ServiceProvider.GetRequiredService<CokerDbContext>();
        var entry = await db.AiMemoryEntries.AsNoTracking().SingleAsync(e => e.ScopeKey == scope.Key && e.MemoryKey == key);
        var revisions = await db.AiMemoryRevisions.AsNoTracking().Where(r => r.EntryId == entry.Id).OrderBy(r => r.Version).ToArrayAsync();
        Require(revisions.Length == 2 && revisions[0].Payload == initial && revisions[1].Payload == entry.Payload &&
            revisions.All(r => r.ActorUserId == 91) && entry.Version == 2 && entry.CreatedAtUtc <= entry.UpdatedAtUtc,
            "Current memory and immutable version history must retain payload, actor and timestamps without extra revisions for rejected writes.");
        var sourceEntry = await db.AiMemoryEntries.AsNoTracking().SingleAsync(e => e.ScopeKey == sourceScope.Key && e.MemoryKey == sourceKey);
        var sourceRevision = await db.AiMemoryRevisions.AsNoTracking().SingleAsync(r => r.EntryId == sourceEntry.Id);
        Require(sourceEntry.SchemaVersion == 2 && sourceEntry.SourceHash == new string('d', 64) && sourceEntry.RuntimeHash == new string('e', 64) &&
            sourceRevision.SourceHash == sourceEntry.SourceHash && sourceRevision.RuntimeHash == sourceEntry.RuntimeHash && sourceEntry.State == "source-analysis",
            "Both current memory and history must retain schema and source/runtime provenance without claiming runtime verification.");
        var entity = db.Model.FindEntityType(typeof(EtheriT.Coker.Core.Models.AiMemoryEntry))!;
        Require(entity.FindProperty("ConcurrencyToken")!.IsConcurrencyToken &&
            entity.GetIndexes().Any(i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "ScopeKey", "MemoryKey" })),
            "The database model must declare optimistic concurrency and unique scoped keys; SQL enforcement needs separate relational verification.");
    }
}
