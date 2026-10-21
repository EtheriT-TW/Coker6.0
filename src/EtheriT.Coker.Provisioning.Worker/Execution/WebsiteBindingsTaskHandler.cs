using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed class WebsiteBindingsTaskHandler(ProcessRunner runner, ProvisioningWorkerOptions options) : IProvisioningTaskHandler
{
    public bool CanHandle(ProvisioningTaskType type) => type == ProvisioningTaskType.SyncWebsiteBindings;

    public async Task<string> ExecuteAsync(ProvisioningTaskDto task, CancellationToken cancellationToken)
    {
        var payload = task.Payload;
        var desired = payload.HostNames?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];
        var previous = payload.PreviousHostNames?.Distinct(StringComparer.OrdinalIgnoreCase).ToArray() ?? [];
        if (string.IsNullOrWhiteSpace(payload.SiteName) || string.IsNullOrWhiteSpace(payload.PrimaryHostName)
            || desired.Length is 0 or > 20 || previous.Length is 0 or > 20
            || !desired.Contains(payload.PrimaryHostName, StringComparer.OrdinalIgnoreCase)
            || !previous.Contains(payload.PrimaryHostName, StringComparer.OrdinalIgnoreCase)
            || desired.Concat(previous).Any(host => Uri.CheckHostName(host) != UriHostNameType.Dns))
            throw new InvalidOperationException("網址繫結同步參數無效。");
        var ssl = new SslTaskHandler(runner, options);
        try
        {
            await ssl.ValidateBindingChangeAsync(task, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"TLS 前置檢查失敗，尚未執行 IIS 繫結變更。\n{ex.Message}", ex);
        }
        var powershell = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
            "System32", "WindowsPowerShell", "v1.0", "powershell.exe");
        // 固定程式碼；所有站名與網址都以JSON環境變數傳入，不插入PowerShell原始碼。
        const string script = """
            $ErrorActionPreference = 'Stop'
            try {
                Add-Type -Path "$env:windir\System32\inetsrv\Microsoft.Web.Administration.dll"
                $data = $env:COKER_BINDING_PAYLOAD | ConvertFrom-Json
                $manager = New-Object Microsoft.Web.Administration.ServerManager
                try {
                    $site = $manager.Sites[$data.SiteName]
                    if ($null -eq $site) { throw 'IIS website not found.' }
                    if (-not @($site.Bindings | Where-Object { $_.Host -ieq $data.PrimaryHostName }).Count) {
                        throw 'Primary hostname no longer belongs to this IIS website.'
                    }
                    foreach ($other in $manager.Sites) {
                        if ($other.Name -ieq $site.Name) { continue }
                        if (@($other.Bindings | Where-Object { $data.HostNames -icontains $_.Host }).Count) {
                            throw 'Hostname is already bound to another IIS website.'
                        }
                    }
                    foreach ($binding in $site.Bindings) {
                        if (($data.PreviousHostNames -icontains $binding.Host) -and
                            (($binding.Protocol -eq 'http' -and $binding.EndPoint.Port -ne 80) -or
                             ($binding.Protocol -eq 'https' -and $binding.EndPoint.Port -ne 443))) {
                            throw 'Custom port bindings require manual review.'
                        }
                    }
                    $remove = @($site.Bindings | Where-Object {
                        $_.Protocol -in @('http', 'https') -and
                        ($data.PreviousHostNames -icontains $_.Host) -and
                        ($data.HostNames -inotcontains $_.Host)
                    })
                    foreach ($binding in $remove) { $site.Bindings.Remove($binding) }
                    foreach ($hostName in $data.HostNames) {
                        if (-not @($site.Bindings | Where-Object { $_.Protocol -eq 'http' -and $_.Host -ieq $hostName }).Count) {
                            $null = $site.Bindings.Add("*:80:$hostName", 'http')
                        }
                    }
                    $manager.CommitChanges()
                    Write-Output 'HTTP bindings synchronized; removed obsolete managed HTTP/HTTPS bindings.'
                } finally { $manager.Dispose() }
            } catch { [Console]::Error.WriteLine($_.Exception.Message); exit 1 }
            """;
        var bindingResult = await runner.RunAsync(powershell,
            ["-NoProfile", "-NonInteractive", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes(script))],
            new Dictionary<string, string?> { ["COKER_BINDING_PAYLOAD"] = JsonSerializer.Serialize(payload) }, cancellationToken);
        try
        {
            // win-acme依更新後的HTTP hosts產生／續期憑證並安裝HTTPS繫結。
            var result = await ssl.ExecuteAsync(task, cancellationToken);
            var appCmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "inetsrv", "appcmd.exe");
            var xml = await runner.RunAsync(appCmd, ["list", "site", "/xml"], null, cancellationToken,
                maxOutputLength: 2_000_000, xmlOutput: true);
            var site = XDocument.Parse(xml).Descendants("SITE").Single(item =>
                string.Equals((string?)item.Attribute("SITE.NAME"), payload.SiteName, StringComparison.OrdinalIgnoreCase));
            var httpsHosts = ((string?)site.Attribute("bindings") ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(binding => binding.StartsWith("https/", StringComparison.OrdinalIgnoreCase))
                .Select(binding => binding[(binding.LastIndexOf(':') + 1)..]).ToArray();
            if (desired.Any(host => !httpsHosts.Contains(host, StringComparer.OrdinalIgnoreCase)))
                throw new InvalidOperationException("win-acme 完成後仍缺少部分 HTTPS 主機名稱繫結，請檢查 renewal 的安裝設定。");
            return $"IIS 網址繫結同步成功，TLS 更新完成。\n{bindingResult}\n{result}";
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new InvalidOperationException($"IIS 網址繫結已同步，但 TLS 更新失敗；不會自動重跑。請至 TLS 管理確認後重新執行 TLS 安裝。\n{ex.Message}", ex);
        }
    }
}
