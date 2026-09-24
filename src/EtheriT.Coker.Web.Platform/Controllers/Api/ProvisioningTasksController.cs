using System.Data;
using System.Net;
using System.Text.Json;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Platform.Models.Provisioning;
using EtheriT.Coker.Web.Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Web.Platform.Controllers.Api;

[ApiController]
[Route("api/provisioning/tasks")]
public sealed class ProvisioningTasksController(CokerDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet("servers")]
    public IReadOnlyList<ProvisioningServerDto> GetServers() => GetConfiguredServers()
        .Select(x => new ProvisioningServerDto(x.Id, string.IsNullOrWhiteSpace(x.DisplayName) ? x.Id : x.DisplayName, x.IsDnsServer))
        .ToList();

    [HttpGet("agents")]
    public async Task<IReadOnlyList<ProvisioningAgentStatusDto>> GetAgents()
    {
        var configuredServers = GetConfiguredServers();
        var serverIds = configuredServers.Select(x => x.Id).ToList();
        var states = await db.ProvisioningAgentStatuses.AsNoTracking()
            .Where(x => serverIds.Contains(x.ServerId))
            .ToDictionaryAsync(x => x.ServerId, StringComparer.OrdinalIgnoreCase, HttpContext.RequestAborted);
        var offlineSeconds = Math.Max(15, configuration.GetValue("Provisioning:AgentOfflineSeconds", 45));
        var onlineThreshold = DateTime.UtcNow.AddSeconds(-offlineSeconds);

        return configuredServers.Select(server =>
        {
            states.TryGetValue(server.Id, out var state);
            return new ProvisioningAgentStatusDto(
                server.Id,
                string.IsNullOrWhiteSpace(server.DisplayName) ? server.Id : server.DisplayName,
                server.IsDnsServer,
                state?.LastSeenAtUtc >= onlineThreshold,
                state?.WorkerId,
                state?.MachineName,
                state?.AgentVersion,
                state?.DryRun,
                state?.CpuUsagePercent,
                state?.MemoryUsedBytes,
                state?.MemoryTotalBytes,
                state?.MemoryUsagePercent,
                DeserializeList<ProvisioningDiskMetricDto>(state?.DiskMetricsJson),
                DeserializeList<ProvisioningAppPoolMetricDto>(state?.AppPoolMetricsJson),
                state?.LastSeenAtUtc);
        }).ToList();
    }

    [HttpGet("agents/{serverId}/metrics")]
    public async Task<ActionResult<ProvisioningMetricHistoryDto>> GetMetrics(string serverId, [FromQuery] int days = 1)
    {
        var configuredServer = GetConfiguredServers().FirstOrDefault(x =>
            string.Equals(x.Id, serverId, StringComparison.OrdinalIgnoreCase));
        if (configuredServer is null) return NotFound();

        days = Math.Clamp(days, 1, 30);
        var toUtc = DateTime.UtcNow;
        var fromUtc = toUtc.AddDays(-days);
        var bucketMinutes = days switch { <= 1 => 5, <= 7 => 30, _ => 60 };
        var samples = await db.ProvisioningMetricSamples.AsNoTracking()
            .Where(x => x.ServerId == configuredServer.Id && x.SampledAtUtc >= fromUtc && x.SampledAtUtc <= toUtc)
            .OrderBy(x => x.SampledAtUtc)
            .ToListAsync(HttpContext.RequestAborted);

        var serverMetrics = samples
            .GroupBy(x => BucketTime(x.SampledAtUtc, bucketMinutes))
            .Select(group => new ProvisioningServerMetricPointDto(
                group.Key,
                Average(group.Select(x => x.CpuUsagePercent)),
                Average(group.Select(x => x.MemoryUsagePercent)),
                AverageLong(group.Select(x => x.MemoryUsedBytes)),
                AverageLong(group.Select(x => x.MemoryTotalBytes))))
            .ToList();
        var diskSamples = samples.SelectMany(sample =>
                DeserializeList<ProvisioningDiskMetricDto>(sample.DiskMetricsJson)
                    .Select(disk => new { sample.SampledAtUtc, Disk = disk }))
            .GroupBy(x => new { x.Disk.Name, x.Disk.VolumeLabel })
            .Select(disk => new ProvisioningDiskMetricSeriesDto(
                disk.Key.Name,
                disk.Key.VolumeLabel,
                disk.GroupBy(x => BucketTime(x.SampledAtUtc, bucketMinutes))
                    .Select(group => new ProvisioningDiskMetricPointDto(
                        group.Key,
                        group.Average(x => x.Disk.UsagePercent),
                        AverageLong(group.Select(x => x.Disk.UsedBytes)) ?? 0,
                        AverageLong(group.Select(x => x.Disk.TotalBytes)) ?? 0,
                        AverageLong(group.Select(x => x.Disk.FreeBytes)) ?? 0))
                    .OrderBy(x => x.SampledAtUtc)
                    .ToList()))
            .OrderBy(x => x.Name)
            .ToList();

        return new ProvisioningMetricHistoryDto(
            configuredServer.Id, fromUtc, toUtc, bucketMinutes, serverMetrics, diskSamples);
    }

    [HttpGet]
    public async Task<IReadOnlyList<ProvisioningTaskDto>> GetList()
    {
        var tasks = await db.ProvisioningTasks.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Take(100)
            .ToListAsync(HttpContext.RequestAborted);
        return tasks.Select(ToDto).ToList();
    }

    [HttpPost]
    public async Task<ActionResult<ProvisioningTaskDto>> Create(CreateProvisioningTaskRequest request)
    {
        Validate(request);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var task = new ProvisioningTask
        {
            TargetServerId = request.TargetServerId.Trim(),
            Type = request.Type,
            Status = ProvisioningTaskStatus.Pending,
            PayloadJson = JsonSerializer.Serialize(new ProvisioningTaskPayload(
                Clean(request.SiteName), request.StartSite, Clean(request.ZoneName),
                Clean(request.RecordName), Clean(request.IPv4Address),
                NormalizeHosts(request.HostNames))),
            CreatedAtUtc = DateTime.UtcNow
        };
        db.ProvisioningTasks.Add(task);
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return Created($"/api/provisioning/tasks/{task.Id}", ToDto(task));
    }

    private void Validate(CreateProvisioningTaskRequest request)
    {
        var servers = GetConfiguredServers();
        if (string.IsNullOrWhiteSpace(request.TargetServerId) ||
            !servers.Any(x => string.Equals(x.Id, request.TargetServerId.Trim(), StringComparison.OrdinalIgnoreCase)))
            ModelState.AddModelError(nameof(request.TargetServerId), "目標伺服器不在允許清單中。");

        if (!Enum.IsDefined(request.Type))
            ModelState.AddModelError(nameof(request.Type), "不支援的任務類型。");

        if (request.Type == ProvisioningTaskType.SetIisSiteState && string.IsNullOrWhiteSpace(request.SiteName))
            ModelState.AddModelError(nameof(request.SiteName), "請輸入 IIS 網站名稱。");

        if (request.Type is ProvisioningTaskType.CreateDnsARecord or ProvisioningTaskType.DeleteDnsARecord)
        {
            if (!servers.Any(x => x.IsDnsServer && string.Equals(x.Id, request.TargetServerId?.Trim(), StringComparison.OrdinalIgnoreCase)))
                ModelState.AddModelError(nameof(request.TargetServerId), "DNS 任務只能交由 DNS Server 執行。");
            if (!IsDnsName(request.ZoneName)) ModelState.AddModelError(nameof(request.ZoneName), "DNS Zone 格式不正確。");
            if (!IsRecordName(request.RecordName)) ModelState.AddModelError(nameof(request.RecordName), "DNS 記錄名稱格式不正確。");
            if (!IPAddress.TryParse(request.IPv4Address, out var address) || address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                ModelState.AddModelError(nameof(request.IPv4Address), "請輸入正確的 IPv4 位址。");
        }

        if (request.Type == ProvisioningTaskType.InstallSsl)
        {
            var hosts = NormalizeHosts(request.HostNames);
            if (hosts.Count == 0) ModelState.AddModelError(nameof(request.HostNames), "請至少輸入一個 SSL 主機名稱。");
            else if (hosts.Count > 100) ModelState.AddModelError(nameof(request.HostNames), "一次最多處理 100 個 SSL 主機名稱。");
            else if (hosts.Any(x => !IsDnsName(x))) ModelState.AddModelError(nameof(request.HostNames), "SSL 主機名稱格式不正確。");
        }
    }

    private static bool IsDnsName(string? value) =>
        !string.IsNullOrWhiteSpace(value) && Uri.CheckHostName(value.Trim()) == UriHostNameType.Dns;

    private static bool IsRecordName(string? value) =>
        !string.IsNullOrWhiteSpace(value) && (value.Trim() == "@" ||
        Uri.CheckHostName(value.Trim()) == UriHostNameType.Dns);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private List<ProvisioningServerOptions> GetConfiguredServers() =>
        (configuration.GetSection("Provisioning:Servers").Get<List<ProvisioningServerOptions>>() ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.Id))
            .ToList();

    private static IReadOnlyList<string> NormalizeHosts(IReadOnlyList<string>? hosts) =>
        (hosts ?? []).SelectMany(x => x.Split([',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    internal static ProvisioningTaskDto ToDto(ProvisioningTask task) => new(
        task.Id, task.TargetServerId, task.Type, task.Status,
        JsonSerializer.Deserialize<ProvisioningTaskPayload>(task.PayloadJson) ?? new(null, null, null, null, null, null),
        task.AttemptCount, task.CreatedAtUtc, task.StartedAtUtc, task.CompletedAtUtc, task.ResultMessage);

    private static IReadOnlyList<T> DeserializeList<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<T>>(json) ?? []; }
        catch (JsonException) { return []; }
    }

    private static DateTime BucketTime(DateTime value, int minutes)
    {
        var ticks = TimeSpan.FromMinutes(minutes).Ticks;
        return new DateTime(value.Ticks / ticks * ticks, DateTimeKind.Utc);
    }

    private static double? Average(IEnumerable<double?> values)
    {
        var available = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
        return available.Count == 0 ? null : Math.Round(available.Average(), 1);
    }

    private static long? AverageLong(IEnumerable<long> values)
    {
        var available = values.ToList();
        return available.Count == 0 ? null : (long)available.Average(x => (double)x);
    }
}

[ApiController]
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("api/provisioning/agent/tasks")]
public sealed class ProvisioningAgentTasksController(
    CokerDbContext db,
    ProvisioningAgentAuthenticator authenticator,
    IConfiguration configuration) : ControllerBase
{
    private const string ApiKeyHeader = "X-Provisioning-Key";

    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat(ProvisioningAgentHeartbeatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ServerId) || string.IsNullOrWhiteSpace(request.WorkerId)) return BadRequest();
        var serverId = request.ServerId.Trim();
        if (!Authenticate(serverId)) return Unauthorized();

        var state = await db.ProvisioningAgentStatuses
            .FirstOrDefaultAsync(x => x.ServerId == serverId, HttpContext.RequestAborted);
        if (state is null)
        {
            state = new ProvisioningAgentStatus { ServerId = serverId };
            db.ProvisioningAgentStatuses.Add(state);
        }

        state.WorkerId = Truncate(request.WorkerId, 200);
        state.MachineName = Truncate(request.MachineName, 200);
        state.AgentVersion = Truncate(request.AgentVersion, 50);
        state.DryRun = request.DryRun;
        state.CpuUsagePercent = ClampPercent(request.CpuUsagePercent);
        state.MemoryUsedBytes = Math.Max(0, request.MemoryUsedBytes);
        state.MemoryTotalBytes = Math.Max(0, request.MemoryTotalBytes);
        state.MemoryUsagePercent = ClampPercent(request.MemoryUsagePercent);
        var disks = (request.Disks ?? []).Take(32).ToList();
        var applicationPools = (request.ApplicationPools ?? []).Take(1000).ToList();
        state.DiskMetricsJson = JsonSerializer.Serialize(disks);
        state.AppPoolMetricsJson = JsonSerializer.Serialize(applicationPools);
        var now = DateTime.UtcNow;
        state.LastSeenAtUtc = now;

        var sampleSeconds = Math.Max(30, configuration.GetValue("Provisioning:HistorySampleSeconds", 60));
        var lastSampleAt = await db.ProvisioningMetricSamples
            .Where(x => x.ServerId == serverId)
            .MaxAsync(x => (DateTime?)x.SampledAtUtc, HttpContext.RequestAborted);
        if (lastSampleAt is null || lastSampleAt <= now.AddSeconds(-sampleSeconds))
        {
            db.ProvisioningMetricSamples.Add(new ProvisioningMetricSample
            {
                ServerId = serverId,
                SampledAtUtc = now,
                CpuUsagePercent = state.CpuUsagePercent,
                MemoryUsedBytes = state.MemoryUsedBytes,
                MemoryTotalBytes = state.MemoryTotalBytes,
                MemoryUsagePercent = state.MemoryUsagePercent,
                DiskMetricsJson = state.DiskMetricsJson
            });
            var retentionDays = Math.Clamp(configuration.GetValue("Provisioning:HistoryRetentionDays", 30), 1, 365);
            var expiresBefore = now.AddDays(-retentionDays);
            await db.ProvisioningMetricSamples
                .Where(x => x.SampledAtUtc < expiresBefore)
                .ExecuteDeleteAsync(HttpContext.RequestAborted);
        }
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPost("claim")]
    public async Task<ActionResult<ProvisioningTaskDto>> Claim(ClaimProvisioningTaskRequest request)
    {
        if (!Authenticate(request.ServerId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.ServerId) || string.IsNullOrWhiteSpace(request.WorkerId)) return BadRequest();

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, HttpContext.RequestAborted);
        var now = DateTime.UtcNow;
        // 無法得知 Worker 中斷前是否已完成主機操作，故逾時任務標記失敗、禁止自動重跑，避免重複重啟或重複異動 DNS。
        await db.ProvisioningTasks
            .Where(x => x.TargetServerId == request.ServerId &&
                x.Status == ProvisioningTaskStatus.Running && x.LeaseExpiresAtUtc < now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, ProvisioningTaskStatus.Failed)
                .SetProperty(x => x.CompletedAtUtc, now)
                .SetProperty(x => x.LeaseExpiresAtUtc, (DateTime?)null)
                .SetProperty(x => x.ResultMessage, "Worker 執行逾時；為避免重複操作，系統未自動重試。請人工確認主機狀態。"),
                HttpContext.RequestAborted);
        var task = await db.ProvisioningTasks
            .Where(x => x.TargetServerId == request.ServerId &&
                x.Status == ProvisioningTaskStatus.Pending)
            .OrderBy(x => x.CreatedAtUtc)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(HttpContext.RequestAborted);

        if (task is null)
        {
            await transaction.CommitAsync(HttpContext.RequestAborted);
            return NoContent();
        }

        task.Status = ProvisioningTaskStatus.Running;
        task.ClaimedBy = request.WorkerId.Trim();
        task.StartedAtUtc ??= now;
        task.LeaseExpiresAtUtc = now.AddMinutes(10);
        task.AttemptCount++;
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);
        return ProvisioningTasksController.ToDto(task);
    }

    [HttpPost("{id:long}/complete")]
    public async Task<IActionResult> Complete(long id, CompleteProvisioningTaskRequest request)
    {
        if (!Authenticate(request.ServerId)) return Unauthorized();
        var task = await db.ProvisioningTasks.FirstOrDefaultAsync(x => x.Id == id, HttpContext.RequestAborted);
        if (task is null) return NotFound();
        if (!string.Equals(task.TargetServerId, request.ServerId, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(task.ClaimedBy, request.WorkerId, StringComparison.Ordinal)) return Conflict();

        task.Status = request.Succeeded ? ProvisioningTaskStatus.Succeeded : ProvisioningTaskStatus.Failed;
        task.CompletedAtUtc = DateTime.UtcNow;
        task.LeaseExpiresAtUtc = null;
        task.ResultMessage = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim()[..Math.Min(request.Message.Trim().Length, 4000)];
        await db.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private bool Authenticate(string? serverId)
    {
        return authenticator.IsValid(serverId, Request.Headers[ApiKeyHeader].FirstOrDefault());
    }

    private static string Truncate(string? value, int length) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim()[..Math.Min(value.Trim().Length, length)];

    private static double? ClampPercent(double? value) =>
        value is null || double.IsNaN(value.Value) || double.IsInfinity(value.Value)
            ? null
            : Math.Clamp(value.Value, 0, 100);
}
