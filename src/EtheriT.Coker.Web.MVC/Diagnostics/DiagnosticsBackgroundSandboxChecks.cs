using System.Reflection;
using Hangfire.Storage;

namespace EtheriT.Coker.Web.MVC.Diagnostics;

public static partial class BackofficeSandboxChecks
{
    private static async Task DiagnosticsBackgroundRegression(BackofficeSandbox box)
    {
        var connection = DispatchProxy.Create<IStorageConnection, DiagnosticsConnectionFixture>();
        var fixture = (DiagnosticsConnectionFixture)(object)connection;
        var store = new DiagnosticsWorkStore(() => connection);
        var work = new DiagnosticsWork("fixture", false, "scope", new() { 0, 2 }) { JobId = "job" };
        store.Save(connection, 1, 1, work);
        Require(store.Load(2, 1) == null && store.Load(1, 2) == null, "Work leaked across account/site.");
        Require(store.Load(1, 1)?.State == "Queued", "Queued work was lost.");
        Require(!store.Update(1, 1, "other-run", current => current with { CancelRequested = true }),
            "A stale run changed the current work.");
        Require(store.Update(1, 1, work.Id, current => current with { State = "Running", Completed = 1 }),
            "Work progress was not saved.");
        var restored = new DiagnosticsWorkStore(() => connection).Load(1, 1);
        Require(restored is { Completed: 1, Active: true } && restored.Targets.Skip(restored.Completed).SequenceEqual(new[] { 2 }),
            "A new instance could not resume after the saved page.");
        store.Update(1, 1, work.Id, current => current with { CancelRequested = true });
        Require(store.Load(1, 1)?.CancelRequested == true, "Stop request did not survive restoration.");
        store.Update(1, 1, work.Id, current => current with { State = "Stopped" });
        Require(!store.Update(1, 1, work.Id, current => current with { State = "Running" }),
            "A terminal run was restarted by a stale worker.");
        store.Save(connection, 1, 1, work);
        fixture.JobState = "Failed";
        Require(store.Load(1, 1) is { State: "Interrupted", Active: false }, "Failed Hangfire job remained active forever.");
        fixture.JobState = null;
        store.Save(connection, 1, 1, work);
        Require(store.Load(1, 1)?.State == "Interrupted", "Missing Hangfire job remained active forever.");
        Require(DiagnosticsWork.SelectTargets(4, true, index => index is 1 or 3).SequenceEqual(new[] { 1, 3 })
            && DiagnosticsWork.SelectTargets(4, false, _ => false).SequenceEqual(new[] { 0, 1, 2, 3 })
            && DiagnosticsWork.SelectTargets(0, false, _ => true).Count == 0,
            "Retry selection broadened or lost the authorized target list.");

        var root = box.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
        var publicStore = new PublicResultStore(root);
        var first = new PublicPageResult("first", "https://example.invalid/first", DateTime.UtcNow, new());
        var second = first with { Title = "second", Url = "https://example.invalid/second" };
        await publicStore.SaveAsync(11, 12, "scope", first);
        await publicStore.SaveAsync(11, 12, "scope", second);
        await publicStore.ResetAsync(11, 12, "scope", new[] { first.Url });
        var report = await new PublicResultStore(root).LoadAsync(11, 12, "scope");
        Require(report != null && !report.Pages.ContainsKey(first.Url) && report.Pages.ContainsKey(second.Url),
            "Retry reset retained old success or deleted an unrelated page.");
    }

    // The production store is exercised without SQL, Hangfire servers or jobs.
    public class DiagnosticsConnectionFixture : DispatchProxy
    {
        private readonly Dictionary<string, Dictionary<string, string>> hashes = new();
        public string? JobState { get; set; } = "Enqueued";
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            switch (method?.Name)
            {
                case "Dispose": return null;
                case "AcquireDistributedLock": return new FixtureLock();
                case "GetAllEntriesFromHash":
                    return hashes.TryGetValue((string)args![0]!, out var values) ? new Dictionary<string, string>(values) : null;
                case "SetRangeInHash":
                    hashes[(string)args![0]!] = ((IEnumerable<KeyValuePair<string, string>>)args[1]!).ToDictionary(pair => pair.Key, pair => pair.Value);
                    return null;
                case "GetStateData": return JobState == null ? null : new StateData { Name = JobState };
                default: throw new InvalidOperationException("Unexpected storage operation: " + method?.Name);
            }
        }
        private sealed class FixtureLock : IDisposable { public void Dispose() { } }
    }
}
