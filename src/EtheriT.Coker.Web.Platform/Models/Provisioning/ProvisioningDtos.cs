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
    IReadOnlyList<ProvisioningAppPoolMetricDto>? ApplicationPools,
    TlsSnapshotDto? TlsSnapshot = null);

public sealed record TlsSnapshotDto(DateTime? CollectedAtUtc, IReadOnlyList<TlsCertificateDto> Certificates, string? Error,
    IReadOnlyList<TlsWebsiteDto>? Websites = null, TlsCentralStoreDto? CentralStore = null, TlsWacsLogsDto? WacsLogs = null,
    TlsWacsScheduleDto? WacsSchedule = null);
public sealed record TlsWacsScheduleDto(DateTime? CollectedAtUtc, string Folder, IReadOnlyList<TlsWacsTaskDto> Tasks, string? Error);
public sealed record TlsWacsTaskDto(string TaskPath, bool Enabled, string State, DateTime? NextRunAtUtc,
    DateTime? LastRunAtUtc, int LastResult);
public sealed record TlsWacsLogsDto(DateTime? CollectedAtUtc, string DirectoryPath, IReadOnlyList<TlsWacsLogFileDto> Files, string? Error,
    IReadOnlyList<TlsWacsExecutionDto>? Executions = null);
public sealed record TlsWacsExecutionDto(string Id, DateTime? StartedAtUtc, DateTime? CompletedAtUtc,
    string TaskName, string Result, string Details, string FileName, int StartLine, int EndLine);
public sealed record TlsWacsLogFileDto(string FileName, DateTime LastWriteAtUtc, int? ErrorEntries, int? WarningEntries, string? Error);
public sealed record TlsCentralStoreDto(DateTime? CollectedAtUtc, bool? Enabled, string? DirectoryPath,
    IReadOnlyList<string> PfxFileNames, string? Error, IReadOnlyList<TlsPfxCertificateDto>? Certificates = null);
public sealed record TlsPfxCertificateDto(string FileName, string? Subject, string? Issuer, string? Thumbprint,
    DateTime? NotBeforeUtc, DateTime? NotAfterUtc, string? Error);
public sealed record TlsWebsiteDto(string SiteName, string State, IReadOnlyList<string> HttpsBindings, IReadOnlyList<string> HttpsUrls);
public sealed record TlsCertificateDto(
    string Thumbprint, string StoreName, string? Subject, string? Issuer,
    IReadOnlyList<string> DnsNames, DateTime? NotBeforeUtc, DateTime? NotAfterUtc,
    bool HasPrivateKey, IReadOnlyList<string> IisBindings, string? Error);
public sealed record ServerTlsStatusDto(string ServerId, bool IsOnline, DateTime? LastSeenAtUtc, TlsSnapshotDto? Snapshot,
    IReadOnlyList<TlsUrlIssueDto>? UrlIssues = null);
public sealed record TlsUrlIssueDto(string Source, string Website, string OriginalUrl, string Reason);

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
