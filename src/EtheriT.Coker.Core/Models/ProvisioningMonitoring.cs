using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models;

public sealed class ProvisioningAgentStatus
{
    [Key]
    [StringLength(100)]
    public string ServerId { get; set; } = string.Empty;
    [StringLength(200)]
    public string WorkerId { get; set; } = string.Empty;
    [StringLength(200)]
    public string MachineName { get; set; } = string.Empty;
    [StringLength(50)]
    public string AgentVersion { get; set; } = string.Empty;
    public bool DryRun { get; set; }
    public double? CpuUsagePercent { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long MemoryTotalBytes { get; set; }
    public double? MemoryUsagePercent { get; set; }
    public string DiskMetricsJson { get; set; } = "[]";
    public string AppPoolMetricsJson { get; set; } = "[]";
    public string? TlsSnapshotJson { get; set; }
    public DateTime LastSeenAtUtc { get; set; }
}

public sealed class ProvisioningMetricSample
{
    public long Id { get; set; }
    [StringLength(100)]
    public string ServerId { get; set; } = string.Empty;
    public DateTime SampledAtUtc { get; set; }
    public double? CpuUsagePercent { get; set; }
    public long MemoryUsedBytes { get; set; }
    public long MemoryTotalBytes { get; set; }
    public double? MemoryUsagePercent { get; set; }
    public string DiskMetricsJson { get; set; } = "[]";
}

/// <summary>同一取樣時間的 IIS 集區資源與對應網站在線人數。</summary>
public sealed class ProvisioningSiteMetricSample
{
    public long Id { get; set; }
    [StringLength(100)] public string ServerId { get; set; } = "";
    [StringLength(200)] public string ApplicationPoolName { get; set; } = "";
    public DateTime SampledAtUtc { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public long WorkingSetBytes { get; set; }
    public double? CpuUsagePercent { get; set; }
    public long? OnlineVisitors { get; set; }
    public string WebsiteIdsJson { get; set; } = "[]";
    public string WebsiteNamesJson { get; set; } = "[]";
    public string ProcessIdsJson { get; set; } = "[]";
    public string? RuntimeMetricsJson { get; set; }
}
