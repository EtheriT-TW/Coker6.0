using System.Globalization;
using System.Text.Json;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

// Store results outside wwwroot. Only the authorized controller reads this file;
// never accept a file name or directory from request parameters.
public sealed class BackofficeResultStore(string contentRoot)
{
    public sealed record Snapshot(string ScopeStamp, DateTime AtUtc, Dictionary<string, BackofficeTestPage> Pages)
    {
        // Internal maintenance evidence only; never include in the result view or API response.
        public Dictionary<string, string> Errors { get; init; } = new();
    }

    private string FileName(long websiteId, long userId)
    {
        if (websiteId <= 0 || userId <= 0) throw new InvalidOperationException("Invalid diagnostics identity.");
        return Path.Combine(contentRoot, "App_Data", "BackofficeDiagnostics",
            websiteId.ToString(CultureInfo.InvariantCulture) + "-" + userId.ToString(CultureInfo.InvariantCulture) + ".json");
    }

    public async Task<Snapshot?> LoadAsync(long websiteId, long userId, string? stamp)
    {
        var path = FileName(websiteId, userId);
        if (!File.Exists(path)) return null;
        var snapshot = JsonSerializer.Deserialize<Snapshot>(await File.ReadAllTextAsync(path));
        if (snapshot == null || (stamp != null && snapshot.ScopeStamp != stamp) || snapshot.Pages == null) return null;
        return snapshot;
    }

    // Background jobs and enqueue/reset hold the durable work lock for read/merge/write.
    public async Task SaveAsync(long websiteId, long userId, string stamp, IEnumerable<BackofficeTestPage> completed,
        IReadOnlyDictionary<string, string>? errors = null)
    {
        Snapshot? previous;
        try { previous = await LoadAsync(websiteId, userId, stamp); }
        catch (JsonException) { previous = null; } // A new run can replace a corrupt record.
        var pages = previous == null ? new Dictionary<string, BackofficeTestPage>() : new Dictionary<string, BackofficeTestPage>(previous.Pages);
        var evidence = previous?.Errors == null ? new Dictionary<string, string>() : new Dictionary<string, string>(previous.Errors);
        foreach (var page in completed)
        {
            pages[page.Id] = page;
            evidence.Remove(page.Id);
            foreach (var test in page.Cases) evidence.Remove(test.Id);
        }
        if (errors != null)
            foreach (var error in errors) evidence[error.Key] = error.Value;
        var path = FileName(websiteId, userId);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(new Snapshot(stamp, DateTime.UtcNow, pages) { Errors = evidence }));
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
