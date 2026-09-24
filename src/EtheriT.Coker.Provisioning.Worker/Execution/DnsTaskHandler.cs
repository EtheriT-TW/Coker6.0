using System.Text;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed class DnsTaskHandler(
    ProcessRunner runner,
    ProvisioningWorkerOptions options) : IProvisioningTaskHandler
{
    public bool CanHandle(ProvisioningTaskType type) =>
        type is ProvisioningTaskType.CreateDnsARecord or ProvisioningTaskType.DeleteDnsARecord;

    public Task<string> ExecuteAsync(ProvisioningTaskDto task, CancellationToken cancellationToken)
    {
        if (!options.DnsEnabled) throw new InvalidOperationException("DNS operations are disabled on this worker.");
        var payload = task.Payload;
        var values = new[] { payload.ZoneName, payload.RecordName, payload.IPv4Address };
        if (values.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("DNS zone, record and IPv4 address are required.");

        const string createScript = "Import-Module DnsServer -ErrorAction Stop; $r=@(Get-DnsServerResourceRecord -ZoneName $env:COKER_DNS_ZONE -Name $env:COKER_DNS_NAME -RRType A -ErrorAction SilentlyContinue); if($r.Count -gt 0){if($r.RecordData.IPv4Address.IPAddressToString -contains $env:COKER_DNS_IP){Write-Output '指定的 DNS A 記錄已存在'; exit 0}; throw '同名 DNS A 記錄已存在，但 IP 不同'}; Add-DnsServerResourceRecordA -ZoneName $env:COKER_DNS_ZONE -Name $env:COKER_DNS_NAME -IPv4Address $env:COKER_DNS_IP -ErrorAction Stop";
        const string deleteScript = "Import-Module DnsServer -ErrorAction Stop; $r=Get-DnsServerResourceRecord -ZoneName $env:COKER_DNS_ZONE -Name $env:COKER_DNS_NAME -RRType A -ErrorAction Stop | Where-Object {$_.RecordData.IPv4Address.IPAddressToString -eq $env:COKER_DNS_IP}; if($null -eq $r){throw '指定的 DNS A 記錄不存在'}; $r | Remove-DnsServerResourceRecord -ZoneName $env:COKER_DNS_ZONE -Force -ErrorAction Stop";
        var script = task.Type == ProvisioningTaskType.DeleteDnsARecord ? deleteScript : createScript;
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        var environment = new Dictionary<string, string?>
        {
            ["COKER_DNS_ZONE"] = payload.ZoneName,
            ["COKER_DNS_NAME"] = payload.RecordName,
            ["COKER_DNS_IP"] = payload.IPv4Address
        };
        var powershell = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
        return runner.RunAsync(
            powershell,
            ["-NoProfile", "-NonInteractive", "-EncodedCommand", encoded],
            environment,
            cancellationToken);
    }
}
