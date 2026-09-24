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
    IReadOnlyList<string>? HostNames);

public sealed record ProvisioningTaskPayload(
    string? SiteName,
    bool? StartSite,
    string? ZoneName,
    string? RecordName,
    string? IPv4Address,
    IReadOnlyList<string>? HostNames);

public sealed class ProvisioningServerOptions
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsDnsServer { get; set; }
}

public sealed record ProvisioningServerDto(string Id, string DisplayName, bool IsDnsServer);

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
