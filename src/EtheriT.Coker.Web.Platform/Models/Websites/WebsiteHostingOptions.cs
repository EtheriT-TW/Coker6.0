using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Web.Platform.Models.Provisioning;

namespace EtheriT.Coker.Web.Platform.Models.Websites;

/// <summary>
/// 網站主機位置、各版本預設主機與預設網址尾碼；對應 appsettings 的 WebsiteHosting。
/// 有 Worker 的主機以 Provisioning:Servers 為主（各台的 HostLocation），這裡只列沒有 Worker 的位置。
/// </summary>
public sealed class WebsiteHostingOptions
{
    public const string SectionName = "WebsiteHosting";

    /// <summary>沒有 Worker 的主機位置（例如 NAS封存）；值會存進 Website.HostLocation，舊資料用的字串不能改。</summary>
    public List<string> ArchiveLocations { get; set; } = [];

    /// <summary>key 用 WebsiteLevelEnum 名稱（簡約／形象／會員／購物），value 必須是可選主機位置之一。</summary>
    public Dictionary<string, string> DefaultHostByLevel { get; set; } = new();

    /// <summary>新網站預設網址尾碼，必須以 . 開頭，例如 .coker.ezsale.tw。</summary>
    public string DefaultUrlSuffix { get; set; } = string.Empty;

    /// <summary>可選主機＝Servers 有填 HostLocation 的伺服器＋ArchiveLocations；由 Program.cs 啟動時組好，不從設定檔綁定。</summary>
    public IReadOnlyList<WebsiteHostLocationOption> AllHostLocations { get; private set; } = [];

    public string? GetDefaultHost(WebsiteLevelEnum level) =>
        DefaultHostByLevel.TryGetValue(level.ToString(), out var host) ? host : null;

    public void LoadHostLocations(IEnumerable<ProvisioningServerOptions> servers)
    {
        // 與監控頁一樣略過沒有 Id 的伺服器；Value 不 Trim，留給驗證擋前後空白
        AllHostLocations = servers
            .Where(server => !string.IsNullOrWhiteSpace(server.Id) && !string.IsNullOrWhiteSpace(server.HostLocation))
            .Select(server => new WebsiteHostLocationOption(server.HostLocation!, server.HostLocation!, server.Id.Trim()))
            .Concat(ArchiveLocations.Select(location => new WebsiteHostLocationOption(location, location, null)))
            .ToList();
    }
}

/// <summary>ServerId 對應 Provisioning:Servers 的 Id；沒有 Worker（ArchiveLocations）為 null。</summary>
public sealed record WebsiteHostLocationOption(string Value, string Text, string? ServerId)
{
    public bool HasWorker => !string.IsNullOrWhiteSpace(ServerId);
}