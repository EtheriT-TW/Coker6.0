using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models;

public enum ProvisioningTaskType
{
    RestartIis = 1,
    SetIisSiteState = 2,
    CreateDnsARecord = 3,
    DeleteDnsARecord = 4,
    InstallSsl = 5
}

public enum ProvisioningTaskStatus
{
    Pending = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3
}

public sealed class ProvisioningTask
{
    public long Id { get; set; }
    [StringLength(100)]
    public string TargetServerId { get; set; } = string.Empty;
    public ProvisioningTaskType Type { get; set; }
    public ProvisioningTaskStatus Status { get; set; }
    public string PayloadJson { get; set; } = "{}";
    [StringLength(200)]
    public string? ClaimedBy { get; set; }
    public DateTime? LeaseExpiresAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    [StringLength(4000)]
    public string? ResultMessage { get; set; }
}
