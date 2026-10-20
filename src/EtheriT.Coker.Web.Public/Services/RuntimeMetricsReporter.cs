using System.Diagnostics;
using System.Runtime;
using System.Text.Json;

namespace EtheriT.Coker.Web.Public.Services;

/// <summary>固定大小的程序診斷快照；不收集網址、IP 或會員資料，不強制 GC。</summary>
public sealed class RuntimeMetricsReporter(IWebHostEnvironment environment, IConfiguration configuration,
    ILogger<RuntimeMetricsReporter> logger) : BackgroundService
{
    public bool Enabled { get; } = configuration.GetValue("Diagnostics:RuntimeMetrics:Enabled", false);
    private long requests;
    private long active;
    public void BeginRequest() { Interlocked.Increment(ref requests); Interlocked.Increment(ref active); }
    public void EndRequest() => Interlocked.Decrement(ref active);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!Enabled) return;
        var directory = Path.Combine(environment.ContentRootPath, "App_Data", "runtime-diagnostics");
        var file = Path.Combine(directory, $"{Environment.ProcessId}.json");
        var temporary = file + ".tmp";
        using var process = Process.GetCurrentProcess();
        var startedAt = process.StartTime.ToUniversalTime();
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        try
        {
            do
            {
                try
                {
                    var info = GC.GetGCMemoryInfo();
                    var snapshot = new
                    {
                        SchemaVersion = 1, ProcessId = Environment.ProcessId, StartedAtUtc = startedAt,
                        SampledAtUtc = DateTime.UtcNow, ServerGc = GCSettings.IsServerGC,
                        ManagedBytes = GC.GetTotalMemory(false), HeapSizeBytes = info.HeapSizeBytes,
                        CommittedBytes = info.TotalCommittedBytes, FragmentedBytes = info.FragmentedBytes,
                        AllocatedBytes = GC.GetTotalAllocatedBytes(false),
                        Gen0 = GC.CollectionCount(0), Gen1 = GC.CollectionCount(1), Gen2 = GC.CollectionCount(2),
                        RequestCount = Interlocked.Read(ref requests), ActiveRequests = Interlocked.Read(ref active)
                    };
                    Directory.CreateDirectory(directory);
                    await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(snapshot), stoppingToken);
                    File.Move(temporary, file, true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    logger.LogWarning(ex, "Unable to write runtime diagnostics snapshot");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally
        {
            try { File.Delete(temporary); File.Delete(file); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            { logger.LogDebug(ex, "Unable to remove runtime diagnostics snapshot"); }
        }
    }
}
