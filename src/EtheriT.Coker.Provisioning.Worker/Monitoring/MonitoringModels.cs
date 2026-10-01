namespace EtheriT.Coker.Provisioning.Worker;

public sealed record AgentHeartbeatRequest(
    string ServerId,
    string WorkerId,
    string MachineName,
    string AgentVersion,
    bool DryRun,
    double? CpuUsagePercent,
    long MemoryUsedBytes,
    long MemoryTotalBytes,
    double? MemoryUsagePercent,
    IReadOnlyList<DiskMetrics> Disks,
    IReadOnlyList<IisApplicationPoolMetrics> ApplicationPools,
    TlsSnapshot? TlsSnapshot = null);

public sealed record SystemMetrics(
    double? CpuUsagePercent,
    long MemoryUsedBytes,
    long MemoryTotalBytes,
    double? MemoryUsagePercent,
    IReadOnlyList<DiskMetrics> Disks,
    IReadOnlyList<IisApplicationPoolMetrics> ApplicationPools,
    TlsSnapshot? TlsSnapshot = null);

public sealed record TlsSnapshot(DateTime? CollectedAtUtc, IReadOnlyList<TlsCertificate> Certificates, string? Error);
public sealed record TlsCertificate(
    string Thumbprint, string StoreName, string? Subject, string? Issuer,
    IReadOnlyList<string> DnsNames, DateTime? NotBeforeUtc, DateTime? NotAfterUtc,
    bool HasPrivateKey, IReadOnlyList<string> IisBindings, string? Error);

public sealed record DiskMetrics(
    string Name,
    string VolumeLabel,
    long UsedBytes,
    long TotalBytes,
    long FreeBytes,
    double UsagePercent);

public sealed record IisSiteBindingMetrics(
    string SiteName,
    IReadOnlyList<string> HostNames,
    string State = "Unknown");

public sealed record IisApplicationPoolMetrics(
    string ApplicationPoolName,
    IReadOnlyList<string> SiteNames,
    IReadOnlyList<string> HostNames,
    string State,
    IReadOnlyList<int> ProcessIds,
    double? CpuUsagePercent,
    long WorkingSetBytes,
    long PrivateMemoryBytes,
    IReadOnlyList<IisSiteBindingMetrics>? SiteBindings = null);
