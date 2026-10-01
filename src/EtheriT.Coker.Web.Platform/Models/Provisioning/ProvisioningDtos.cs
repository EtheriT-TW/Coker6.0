using EtheriT.Coker.Core.Models;

namespace EtheriT.Coker.Web.Platform.Models.Provisioning;

public sealed record CreateProvisioningTaskRequest(
    string TargetServerId,
    ProvisioningTaskType Type,
    string? SiteName,
    bool? StartSite,
    string? ZoneName,
    string? RecordName,
    string? IPv4Address,
    IReadOnlyList<string>? HostNames,
    string? DnsRecordType = null,
    string? DnsRecordValue = null,
    int? MxPreference = null);

public sealed record ProvisioningTaskPayload(
    string? SiteName,
    bool? StartSite,
    string? ZoneName,
    string? RecordName,
    string? IPv4Address,
    IReadOnlyList<string>? HostNames,
    string? DnsRecordType = null,
    string? DnsRecordValue = null,
    int? MxPreference = null);

public sealed class ProvisioningServerOptions
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsDnsServer { get; set; }
    public string[] AllowedDnsZones { get; set; } = [];
}

public sealed record ProvisioningServerDto(string Id, string DisplayName, bool IsDnsServer, IReadOnlyList<string> AllowedDnsZones);

public sealed record ProvisioningTaskDto(
    long Id,
    string TargetServerId,
    ProvisioningTaskType Type,
    ProvisioningTaskStatus Status,
    ProvisioningTaskPayload Payload,
    int AttemptCount,
    DateTime CreatedAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    string? ResultMessage);

public sealed record ClaimProvisioningTaskRequest(string ServerId, string WorkerId);
public sealed record CompleteProvisioningTaskRequest(string ServerId, string WorkerId, bool Succeeded, string? Message);

public sealed record ProvisioningAgentHeartbeatRequest(
    string ServerId,
    string WorkerId,
    string MachineName,
    string AgentVersion,
    bool DryRun,
    double? CpuUsagePercent,
    long MemoryUsedBytes,
    long MemoryTotalBytes,
    double? MemoryUsagePercent,
    IReadOnlyList<ProvisioningDiskMetricDto>? Disks,
    IReadOnlyList<ProvisioningAppPoolMetricDto>? ApplicationPools);

public sealed record ProvisioningDiskMetricDto(
    string Name,
    string VolumeLabel,
    long UsedBytes,
    long TotalBytes,
    long FreeBytes,
    double UsagePercent);

public sealed record ProvisioningIisSiteBindingDto(
    string SiteName,
    IReadOnlyList<string> HostNames,
    string State = "Unknown");

public sealed record ProvisioningAppPoolMetricDto(
    string ApplicationPoolName,
    IReadOnlyList<string> SiteNames,
    IReadOnlyList<string>? HostNames,
    string State,
    IReadOnlyList<int> ProcessIds,
    double? CpuUsagePercent,
    long WorkingSetBytes,
    long PrivateMemoryBytes,
    IReadOnlyList<string>? WebsiteNames = null,
    IReadOnlyList<ProvisioningIisSiteBindingDto>? SiteBindings = null);

public sealed record ProvisioningAgentStatusDto(
    string ServerId,
    string DisplayName,
    bool IsDnsServer,
    bool IsOnline,
    string? WorkerId,
    string? MachineName,
    string? AgentVersion,
    bool? DryRun,
    double? CpuUsagePercent,
    long? MemoryUsedBytes,
    long? MemoryTotalBytes,
    double? MemoryUsagePercent,
    IReadOnlyList<ProvisioningDiskMetricDto> Disks,
    IReadOnlyList<ProvisioningAppPoolMetricDto> ApplicationPools,
    DateTime? LastSeenAtUtc);

public sealed record ProvisioningMetricHistoryDto(
    string ServerId,
    DateTime FromUtc,
    DateTime ToUtc,
    int BucketMinutes,
    IReadOnlyList<ProvisioningServerMetricPointDto> ServerMetrics,
    IReadOnlyList<ProvisioningDiskMetricSeriesDto> Disks);

public sealed record ProvisioningServerMetricPointDto(
    DateTime SampledAtUtc,
    double? CpuUsagePercent,
    double? MemoryUsagePercent,
    long? MemoryUsedBytes,
    long? MemoryTotalBytes);

public sealed record ProvisioningDiskMetricSeriesDto(
    string Name,
    string VolumeLabel,
    IReadOnlyList<ProvisioningDiskMetricPointDto> Points);

public sealed record ProvisioningDiskMetricPointDto(
    DateTime SampledAtUtc,
    double UsagePercent,
    long UsedBytes,
    long TotalBytes,
    long FreeBytes);
