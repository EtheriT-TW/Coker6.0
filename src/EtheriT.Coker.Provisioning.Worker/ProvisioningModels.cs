namespace EtheriT.Coker.Provisioning.Worker;

public sealed class ProvisioningWorkerOptions
{
    public string ServerId { get; set; } = string.Empty;
    public string PlatformBaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int PollingSeconds { get; set; } = 5;
    public int OperationTimeoutSeconds { get; set; } = 300;
    public bool DryRun { get; set; }
    public bool DnsEnabled { get; set; }
    public bool SslEnabled { get; set; } = true;
    public string WacsPath { get; set; } = @"C:\Program Files\win-acme\wacs.exe";
    public string AcmeEmail { get; set; } = string.Empty;
}

public enum ProvisioningTaskType
{
    RestartIis = 1,
    SetIisSiteState = 2,
    CreateDnsARecord = 3,
    DeleteDnsARecord = 4,
    InstallSsl = 5
}

public sealed record ProvisioningTaskPayload(
    string? SiteName,
    bool? StartSite,
    string? ZoneName,
    string? RecordName,
    string? IPv4Address,
    IReadOnlyList<string>? HostNames);

public sealed record ProvisioningTaskDto(long Id, ProvisioningTaskType Type, ProvisioningTaskPayload Payload);
public sealed record ClaimRequest(string ServerId, string WorkerId);
public sealed record CompleteRequest(string ServerId, string WorkerId, bool Succeeded, string Message);
