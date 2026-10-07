namespace EtheriT.Coker.Provisioning.Worker;

public sealed class ProvisioningWorkerOptions
{
    public string ServerId { get; set; } = string.Empty;
    public string PlatformBaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int PollingSeconds { get; set; } = 5;
    public int MonitoringSeconds { get; set; } = 15;
    // Daily sampling hour in the worker server's local time (0–23).
    public int TlsSamplingHour { get; set; } = 3;
    // Optional PFX password; never included in heartbeat payloads or logs.
    public string TlsPfxPassword { get; set; } = string.Empty;
    public string WacsLogDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "win-acme", "acme-v02.api.letsencrypt.org", "Log");
    public int OperationTimeoutSeconds { get; set; } = 300;
    public bool DryRun { get; set; }
    public bool DnsEnabled { get; set; }
    public bool SslEnabled { get; set; } = true;
    public string WacsPath { get; set; } = @"C:\Program Files\win-acme\wacs.exe";
    public string WacsTaskFolder { get; set; } = @"\";
    // Must match the ConfigPath of the win-acme instance invoked by WacsPath.
    public string WacsRenewalDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "win-acme", "acme-v02.api.letsencrypt.org");
    public string AcmeEmail { get; set; } = string.Empty;
}

public enum ProvisioningTaskType
{
    RestartIis = 1,
    SetIisSiteState = 2,
    CreateDnsARecord = 3,
    DeleteDnsARecord = 4,
    InstallSsl = 5,
    CreateDnsRecord = 6,
    DeleteDnsRecord = 7
}

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

public sealed record ProvisioningTaskDto(long Id, ProvisioningTaskType Type, ProvisioningTaskPayload Payload);
public sealed record ClaimRequest(string ServerId, string WorkerId);
public sealed record CompleteRequest(string ServerId, string WorkerId, bool Succeeded, string Message);
