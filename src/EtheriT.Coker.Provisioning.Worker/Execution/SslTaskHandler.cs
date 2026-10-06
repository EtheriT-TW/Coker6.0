using System.Xml.Linq;
using Microsoft.Win32;

namespace EtheriT.Coker.Provisioning.Worker;

public sealed class SslTaskHandler(ProcessRunner runner, ProvisioningWorkerOptions options) : IProvisioningTaskHandler
{
    public bool CanHandle(ProvisioningTaskType type) => type == ProvisioningTaskType.InstallSsl;

    public async Task<string> ExecuteAsync(ProvisioningTaskDto task, CancellationToken cancellationToken)
    {
        if (!options.SslEnabled) throw new InvalidOperationException("SSL operations are disabled on this worker.");
        if (string.IsNullOrWhiteSpace(task.Payload.SiteName))
            throw new InvalidOperationException("TLS 安裝必須指定單一 IIS 網站；請由監控頁重新送出任務。");
        var appCmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "inetsrv", "appcmd.exe");
        const int limit = 2_000_000;
        var xml = await runner.RunAsync(appCmd, ["list", "site", "/xml"], null, cancellationToken,
            maxOutputLength: limit, xmlOutput: true);
        if (xml.Length >= limit) throw new InvalidOperationException("IIS 網站清單超過讀取上限。");
        var sites = XDocument.Parse(xml).Descendants("SITE").ToArray();
        var site = sites.SingleOrDefault(item => string.Equals((string?)item.Attribute("SITE.NAME"),
            task.Payload.SiteName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("指定的 IIS 網站不存在。");
        var hosts = (task.Payload.HostNames ?? []).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (hosts.Length == 0 || hosts.Any(host => !WacsRenewalInventory.Hosts(site).Contains(host, StringComparer.OrdinalIgnoreCase)))
            throw new InvalidOperationException("網域必須全部屬於指定網站目前的 bindings；請重新整理後再操作。");

        var inventory = WacsRenewalInventory.Read(options);
        var matches = inventory.Where(renewal => renewal.AllSites && WacsRenewalInventory.Matches(renewal, site)
            || renewal.SiteIds.Contains(WacsRenewalInventory.SiteId(site))).ToArray();
        if (matches.Length > 1)
            throw new InvalidOperationException("此站對應多筆 renewal，請先確認並整理重複設定；未建立或執行額外續期。");
        if (matches.Length > 0)
        {
            var outputs = new List<string>();
            foreach (var renewal in matches)
            {
                var output = await RunWacsAsync(
                    ["--renew", "--id", renewal.Id, "--force", "--closeonfinish"], cancellationToken);
                outputs.Add($"續期 {renewal.Id}：{WacsRenewalInventory.Advice(renewal)}\n{output}");
            }
            if (!hosts.All(host => matches.Any(renewal => renewal.Hosts.Length == 0
                || renewal.Hosts.Contains(host, StringComparer.OrdinalIgnoreCase))))
                outputs.Add("部分網域尚未涵蓋；已沿用原設定續期，請編輯原 renewal 加入網域，不另建重複設定。");
            return string.Join("\n", outputs);
        }
        if (inventory.Any(renewal => sites.Where(other => WacsRenewalInventory.Matches(renewal, other))
            .SelectMany(WacsRenewalInventory.Hosts).Any(host => hosts.Contains(host, StringComparer.OrdinalIgnoreCase)
                && (renewal.Hosts.Length == 0 || renewal.Hosts.Contains(host, StringComparer.OrdinalIgnoreCase)))))
            throw new InvalidOperationException("網域已由其他網站的 renewal 管理，請先整理來源設定。");
        if (string.IsNullOrWhiteSpace(options.AcmeEmail) || string.IsNullOrWhiteSpace(options.TlsPfxPassword))
            throw new InvalidOperationException("建立中央存放區憑證需要設定 AcmeEmail 與 TlsPfxPassword。");
        using var machine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        using var key = machine.OpenSubKey(@"SOFTWARE\Microsoft\IIS\CentralCertProvider", writable: false);
        var directory = key?.GetValue("CertStoreLocation") as string;
        if (key?.GetValue("Enabled") is not int enabled || enabled == 0
            || string.IsNullOrWhiteSpace(directory) || !Path.IsPathFullyQualified(directory))
            throw new InvalidOperationException("請先啟用並設定 IIS 中央憑證存放區。");
        var id = "coker-site-" + WacsRenewalInventory.SiteId(site);
        if (inventory.Any(renewal => renewal.Id == id))
            throw new InvalidOperationException("固定 renewal ID 已存在，請確認來源設定。");
        return await RunWacsAsync(
            ["--source", "iis", "--siteid", WacsRenewalInventory.SiteId(site),
             "--id", id, "--friendlyname", "Coker website " + task.Payload.SiteName,
             "--installation", "iis", "--store", "centralssl", "--centralsslstore", directory,
             "--pfxpassword", options.TlsPfxPassword, "--emailaddress", options.AcmeEmail,
             "--accepttos", "--closeonfinish"], cancellationToken);
    }

    private async Task<string> RunWacsAsync(string[] arguments, CancellationToken cancellationToken)
    {
        string Redact(string value) => string.IsNullOrEmpty(options.TlsPfxPassword)
            ? value : value.Replace(options.TlsPfxPassword, "[已遮蔽]", StringComparison.Ordinal);
        try { return Redact(await runner.RunAsync(options.WacsPath, arguments, null, cancellationToken)); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { throw new InvalidOperationException(Redact(ex.Message)); }
    }
}
