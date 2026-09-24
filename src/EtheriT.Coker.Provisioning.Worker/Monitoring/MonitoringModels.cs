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
    IReadOnlyList<IisApplicationPoolMetrics> ApplicationPools);

public sealed record SystemMetrics(
    double? CpuUsagePercent,
    long MemoryUsedBytes,
    long MemoryTotalBytes,
    double? MemoryUsagePercent,
    IReadOnlyList<DiskMetrics> Disks,
    IReadOnlyList<IisApplicationPoolMetrics> ApplicationPools);

public sealed record DiskMetrics(
    string Name,
    string VolumeLabel,
    long UsedBytes,
    long TotalBytes,
    long FreeBytes,
    double UsagePercent);

public sealed record IisApplicationPoolMetrics(
    string ApplicationPoolName,
    IReadOnlyList<string> SiteNames,
    string State,
    IReadOnlyList<int> ProcessIds,
    double? CpuUsagePercent,
    long WorkingSetBytes,
    long PrivateMemoryBytes);
