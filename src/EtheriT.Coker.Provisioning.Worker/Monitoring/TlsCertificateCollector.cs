using System.Text;
using System.Text.Json;

namespace EtheriT.Coker.Provisioning.Worker;

/// <summary>Reads local certificate metadata and IIS bindings; never exports private keys or changes bindings.</summary>
public sealed class TlsCertificateCollector(ProcessRunner runner)
{
    private TlsSnapshot? cached;
    public TlsSnapshot? CurrentSnapshot => Volatile.Read(ref cached);
    public void RestoreSnapshot(TlsSnapshot? snapshot) => Volatile.Write(ref cached, snapshot);

    public async Task<TlsSnapshot> CollectAsync(CancellationToken cancellationToken)
    {
        TlsSnapshot snapshot;
        try
        {
            if (!OperatingSystem.IsWindows()) throw new InvalidOperationException("TLS inventory requires Windows.");
            const string script = """
                $ErrorActionPreference = 'Stop'
                [Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
                $bindings = @()
                $bindingError = $null
                try {
                    Import-Module WebAdministration -ErrorAction Stop
                    foreach ($site in @(Get-Website)) {
                        foreach ($binding in @(Get-WebBinding -Name $site.Name -Protocol https)) {
                            $hash = $binding.GetAttributeValue('certificateHash')
                            if ($hash -is [byte[]]) { $hash = ($hash | ForEach-Object { $_.ToString('X2') }) -join '' }
                            $store = [string]$binding.GetAttributeValue('certificateStoreName')
                            if (-not $store) { $store = 'My' }
                            $bindings += [pscustomobject]@{
                                Thumbprint = ([string]$hash).Replace(' ', '').ToUpperInvariant()
                                StoreName = $store
                                Label = "$($site.Name) | $($binding.bindingInformation)"
                                CentralStore = (([int]$binding.GetAttributeValue('sslFlags') -band 2) -ne 0)
                            }
                        }
                    }
                } catch { $bindingError = "IIS binding 讀取失敗：$($_.Exception.Message)" }
                $certificates = @()
                foreach ($store in @('My', 'WebHosting')) {
                    $path = "Cert:\LocalMachine\$store"
                    if (-not (Test-Path $path)) { continue }
                    foreach ($certificate in @(Get-ChildItem $path)) {
                        $linked = @($bindings | Where-Object { $_.Thumbprint -eq $certificate.Thumbprint -and $_.StoreName -eq $store -and -not $_.CentralStore })
                        $certificates += [pscustomobject]@{
                            Thumbprint = $certificate.Thumbprint
                            StoreName = $store
                            Subject = $certificate.Subject
                            Issuer = $certificate.Issuer
                            DnsNames = @($certificate.DnsNameList | ForEach-Object { $_.Unicode })
                            NotBeforeUtc = $certificate.NotBefore.ToUniversalTime().ToString('o')
                            NotAfterUtc = $certificate.NotAfter.ToUniversalTime().ToString('o')
                            HasPrivateKey = $certificate.HasPrivateKey
                            IisBindings = @($linked | ForEach-Object { $_.Label })
                            Error = $null
                        }
                    }
                }
                foreach ($binding in $bindings) {
                    if ($binding.CentralStore -or -not @($certificates | Where-Object { $_.Thumbprint -eq $binding.Thumbprint -and $_.StoreName -eq $binding.StoreName -and $_.NotAfterUtc }).Count) {
                        $certificates += [pscustomobject]@{
                            Thumbprint = $binding.Thumbprint
                            StoreName = $binding.StoreName
                            Subject = $null
                            Issuer = $null
                            DnsNames = @()
                            NotBeforeUtc = $null
                            NotAfterUtc = $null
                            HasPrivateKey = $false
                            IisBindings = @($binding.Label)
                            Error = $(if ($binding.CentralStore) { '此綁定使用集中式憑證存放區，尚未支援讀取' } else { '找不到 IIS 綁定對應的本機憑證' })
                        }
                    }
                }
                [pscustomobject]@{
                    CollectedAtUtc = [DateTime]::UtcNow.ToString('o')
                    Certificates = @($certificates)
                    Error = $bindingError
                } | ConvertTo-Json -Depth 6 -Compress
                """;
            var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
            var output = await runner.RunAsync(powershell,
                ["-NoProfile", "-NonInteractive", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes(script))],
                null, cancellationToken, maxOutputLength: 2_000_000);
            snapshot = JsonSerializer.Deserialize<TlsSnapshot>(output)
                ?? throw new InvalidOperationException("TLS inventory returned no data.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var previous = CurrentSnapshot;
            snapshot = new TlsSnapshot(previous?.CollectedAtUtc, previous?.Certificates ?? [], ex.Message);
        }
        Volatile.Write(ref cached, snapshot);
        return snapshot;
    }
}
