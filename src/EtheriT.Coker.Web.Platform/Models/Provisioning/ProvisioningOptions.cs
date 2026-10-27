using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Web.Platform.Models.Provisioning;

/// <summary>統一從 Provisioning 讀取伺服器與網站預設設定。</summary>
public sealed class ProvisioningOptions
{
    public const string SectionName = "Provisioning";
    public List<ProvisioningServerOptions> Servers { get; set; } = [];
    public string DefaultUrlSuffix { get; set; } = string.Empty;

    public IReadOnlyList<WebsiteHostLocationOption> AllHostLocations => Servers
        .Where(server => !string.IsNullOrWhiteSpace(server.HostLocation))
        .Select(server => new WebsiteHostLocationOption(server.HostLocation!,
            string.IsNullOrWhiteSpace(server.DisplayName) ? server.HostLocation! : server.DisplayName,
            server.ShowInMonitoring ? server.Id : null)).ToList();

    public string? GetDefaultHost(WebsiteLevelEnum level)
    {
        var hosts = Servers.Where(server => !string.IsNullOrWhiteSpace(server.HostLocation)
            && server.SupportedLevels.Contains(level.ToString())).ToArray();
        return hosts.Length == 1 ? hosts[0].HostLocation : null;
    }
}

/// <summary>無監控／Worker 的位置沒有 ServerId，仍可供網站選取。</summary>
public sealed record WebsiteHostLocationOption(string Value, string Text, string? ServerId)
{
    public bool HasWorker => !string.IsNullOrWhiteSpace(ServerId);
}
