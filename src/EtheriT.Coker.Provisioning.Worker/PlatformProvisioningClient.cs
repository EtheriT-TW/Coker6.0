using System.Net.Http.Json;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed class PlatformProvisioningClient
{
    private const string ApiKeyHeader = "X-Provisioning-Key";
    private readonly HttpClient client;
    private readonly ProvisioningWorkerOptions options;

    public PlatformProvisioningClient(ProvisioningWorkerOptions options)
    {
        this.options = options;
        WorkerId = $"{Environment.MachineName}:{Environment.ProcessId}";
        client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(Math.Max(30, options.OperationTimeoutSeconds))
        };
        if (Uri.TryCreate(options.PlatformBaseUrl, UriKind.Absolute, out var baseUri))
            client.BaseAddress = baseUri;
        if (!string.IsNullOrWhiteSpace(options.ApiKey))
            client.DefaultRequestHeaders.Add(ApiKeyHeader, options.ApiKey);
    }

    public string WorkerId { get; }

    public void ValidateConfiguration()
    {
        if (client.BaseAddress is null) throw new InvalidOperationException("ProvisioningWorker:PlatformBaseUrl is invalid.");
        if (string.IsNullOrWhiteSpace(options.ServerId)) throw new InvalidOperationException("ProvisioningWorker:ServerId is required.");
        if (string.IsNullOrWhiteSpace(options.ApiKey)) throw new InvalidOperationException("ProvisioningWorker:ApiKey is required.");
    }

    public async Task<ProvisioningTaskDto?> ClaimAsync(CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/provisioning/agent/tasks/claim",
            new ClaimRequest(options.ServerId, WorkerId), cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProvisioningTaskDto>(cancellationToken: cancellationToken);
    }

    public async Task CompleteAsync(long taskId, bool succeeded, string message, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(
            $"/api/provisioning/agent/tasks/{taskId}/complete",
            new CompleteRequest(options.ServerId, WorkerId, succeeded, message), cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendHeartbeatAsync(SystemMetrics metrics, string agentVersion, CancellationToken cancellationToken)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/provisioning/agent/tasks/heartbeat",
            new AgentHeartbeatRequest(
                options.ServerId,
                WorkerId,
                Environment.MachineName,
                agentVersion,
                options.DryRun,
                metrics.CpuUsagePercent,
                metrics.MemoryUsedBytes,
                metrics.MemoryTotalBytes,
                metrics.MemoryUsagePercent,
                metrics.Disks,
                metrics.ApplicationPools),
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
