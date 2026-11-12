using System.Text.Json;
using Hangfire;
using Hangfire.Storage;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

// One durable run per website/account. Stored in the existing Hangfire storage,
// never selected by a caller-supplied Hangfire job ID.
public sealed record DiagnosticsWork(string Id, bool PublicScan, string Stamp, List<int> Targets)
{
    public bool FunctionScan { get; init; }
    public string? JobId { get; init; }
    public string State { get; init; } = "Queued";
    public int Completed { get; init; }
    public bool CancelRequested { get; init; }
    public string Message { get; init; } = "已排入背景檢測，可離開頁面，稍後返回查看。";
    public DateTime UpdatedUtc { get; init; } = DateTime.UtcNow;
    public bool Active => State is "Queued" or "Running";

    public static List<int> SelectTargets(int count, bool retryOnly, Func<int, bool> needsRetry) =>
        Enumerable.Range(0, count).Where(index => !retryOnly || needsRetry(index)).ToList();
}

public sealed class DiagnosticsWorkStore
{
    private readonly Func<IStorageConnection> connect;
    public DiagnosticsWorkStore(JobStorage storage) : this(storage.GetConnection) { }
    internal DiagnosticsWorkStore(Func<IStorageConnection> connect) => this.connect = connect;
    private static string Key(long site, long user)
    {
        if (site <= 0 || user <= 0) throw new InvalidOperationException("Invalid diagnostics identity.");
        return $"coker:diagnostics:{site}:{user}";
    }

    public IDisposable Lock(IStorageConnection connection, long site, long user) =>
        connection.AcquireDistributedLock(Key(site, user) + ":lock", TimeSpan.FromSeconds(10));

    public IStorageConnection Connect() => connect();

    public DiagnosticsWork? Read(IStorageConnection connection, long site, long user)
    {
        var values = connection.GetAllEntriesFromHash(Key(site, user));
        return values != null && values.TryGetValue("run", out var json)
            ? JsonSerializer.Deserialize<DiagnosticsWork>(json) : null;
    }

    public void Save(IStorageConnection connection, long site, long user, DiagnosticsWork work) =>
        connection.SetRangeInHash(Key(site, user), new Dictionary<string, string>
        {
            ["run"] = JsonSerializer.Serialize(work with { UpdatedUtc = DateTime.UtcNow })
        });

    public DiagnosticsWork? Load(long site, long user)
    {
        using var connection = Connect();
        using var gate = Lock(connection, site, user);
        var work = Read(connection, site, user);
        if (work is { Active: true, State: "Queued", JobId: null } &&
            DateTime.UtcNow - work.UpdatedUtc > TimeSpan.FromMinutes(2))
        {
            work = work with { State = "Interrupted", Message = "背景檢測提交中斷，請重新開始。" };
            Save(connection, site, user, work);
        }
        if (work?.Active == true && work.JobId != null)
        {
            var state = connection.GetStateData(work.JobId)?.Name;
            if (state is null or "Failed" or "Deleted" or "Succeeded")
            {
                work = work with { State = "Interrupted", Message = "背景檢測已中斷，已保存的結果仍保留，請重新檢測。" };
                Save(connection, site, user, work);
            }
        }
        return work;
    }

    public bool Update(long site, long user, string id, Func<DiagnosticsWork, DiagnosticsWork> update)
    {
        using var connection = Connect();
        using var gate = Lock(connection, site, user);
        var work = Read(connection, site, user);
        if (work == null || work.Id != id || !work.Active) return false;
        Save(connection, site, user, update(work));
        return true;
    }
}
