using System.Text;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed class DnsTaskHandler(
    ProcessRunner runner,
    ProvisioningWorkerOptions options) : IProvisioningTaskHandler
{
    public bool CanHandle(ProvisioningTaskType type) =>
        type is ProvisioningTaskType.CreateDnsARecord or ProvisioningTaskType.DeleteDnsARecord or
            ProvisioningTaskType.CreateDnsRecord or ProvisioningTaskType.DeleteDnsRecord;

    public Task<string> ExecuteAsync(ProvisioningTaskDto task, CancellationToken cancellationToken)
    {
        if (!options.DnsEnabled) throw new InvalidOperationException("DNS operations are disabled on this worker.");
        var payload = task.Payload;
        var legacy = task.Type is ProvisioningTaskType.CreateDnsARecord or ProvisioningTaskType.DeleteDnsARecord;
        var recordType = legacy ? "A" : payload.DnsRecordType?.Trim().ToUpperInvariant();
        var value = legacy ? payload.IPv4Address : payload.DnsRecordValue;
        if (string.IsNullOrWhiteSpace(payload.ZoneName) || string.IsNullOrWhiteSpace(payload.RecordName) || string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("DNS zone, record name and value are required.");
        if (recordType is not ("A" or "TXT" or "MX" or "CNAME"))
            throw new InvalidOperationException("Unsupported DNS record type.");
        if (recordType == "MX" && (payload.MxPreference is null or < 0 or > 65535))
            throw new InvalidOperationException("MX preference must be between 0 and 65535.");

        // User input is passed through environment variables, never interpolated into script.
        // Resource-record operations only: this handler never creates a DNS zone.
        const string script = """
            $ErrorActionPreference = 'Stop'
            Import-Module DnsServer -ErrorAction Stop
            $zone = $env:COKER_DNS_ZONE
            $name = $env:COKER_DNS_NAME
            if ($name -eq '@') { $name = '.' }
            $type = $env:COKER_DNS_TYPE
            $value = $env:COKER_DNS_VALUE
            Get-DnsServerZone -Name $zone -ErrorAction Stop | Out-Null
            $records = @(Get-DnsServerResourceRecord -ZoneName $zone -ErrorAction Stop | Where-Object {
                ($_.HostName -eq $name -or ($name -eq '.' -and $_.HostName -eq '@')) -and $_.RecordType -eq $type
            })
            $matching = @($records | Where-Object {
                switch ($type) {
                    'A' { $_.RecordData.IPv4Address.IPAddressToString -eq $value }
                    'TXT' { ($_.RecordData.DescriptiveText -join '') -ceq $value }
                    'MX' { $_.RecordData.MailExchange.TrimEnd('.') -ieq $value.TrimEnd('.') -and $_.RecordData.Preference -eq [int]$env:COKER_DNS_PRIORITY }
                    'CNAME' { $_.RecordData.HostNameAlias.TrimEnd('.') -ieq $value.TrimEnd('.') }
                }
            })
            if ($env:COKER_DNS_DELETE -eq 'true') {
                if ($matching.Count -eq 0) { throw '指定的 DNS 記錄不存在' }
                $matching | Remove-DnsServerResourceRecord -ZoneName $zone -Force -ErrorAction Stop
                Write-Output '已刪除指定的 DNS 記錄'
                exit 0
            }
            if ($matching.Count -gt 0) { Write-Output '指定的 DNS 記錄已存在'; exit 0 }
            if ($records.Count -gt 0 -and $type -in @('A', 'CNAME')) { throw '同名 DNS 記錄已存在，但目標值不同' }
            switch ($type) {
                'A' { Add-DnsServerResourceRecordA -ZoneName $zone -Name $name -IPv4Address $value -ErrorAction Stop }
                'TXT' { Add-DnsServerResourceRecord -ZoneName $zone -Name $name -Txt -DescriptiveText $value -ErrorAction Stop }
                'MX' { Add-DnsServerResourceRecordMX -ZoneName $zone -Name $name -MailExchange $value -Preference ([uint16]$env:COKER_DNS_PRIORITY) -ErrorAction Stop }
                'CNAME' { Add-DnsServerResourceRecordCName -ZoneName $zone -Name $name -HostNameAlias $value -ErrorAction Stop }
            }
            Write-Output '已建立 DNS 記錄'
            """;
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        var environment = new Dictionary<string, string?>
        {
            ["COKER_DNS_ZONE"] = payload.ZoneName,
            ["COKER_DNS_NAME"] = payload.RecordName,
            ["COKER_DNS_TYPE"] = recordType,
            ["COKER_DNS_VALUE"] = value,
            ["COKER_DNS_PRIORITY"] = payload.MxPreference?.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["COKER_DNS_DELETE"] = task.Type is ProvisioningTaskType.DeleteDnsARecord or ProvisioningTaskType.DeleteDnsRecord ? "true" : "false"
        };
        var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
        return runner.RunAsync(powershell, ["-NoProfile", "-NonInteractive", "-EncodedCommand", encoded], environment, cancellationToken);
    }
}
