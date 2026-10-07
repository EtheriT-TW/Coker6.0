using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Web.Platform.Models.Websites;

/// <summary>網站主機位置、各版本預設主機與預設網址尾碼；對應 appsettings 的 WebsiteHosting。</summary>
public sealed class WebsiteHostingOptions
{
    public const string SectionName = "WebsiteHosting";

    /// <summary>可選主機；Value 會存進 Website.HostLocation，舊資料用的字串不能改。</summary>
    public List<WebsiteHostLocationOption> HostLocations { get; set; } = [];

    /// <summary>key 用 WebsiteLevelEnum 名稱（簡約／形象／會員／購物），value 必須是 HostLocations 的 Value。</summary>
    public Dictionary<string, string> DefaultHostByLevel { get; set; } = new();

    /// <summary>新網站預設網址尾碼，必須以 . 開頭，例如 .coker.ezsale.tw。</summary>
    public string DefaultUrlSuffix { get; set; } = string.Empty;

    public string? GetDefaultHost(WebsiteLevelEnum level) =>
        DefaultHostByLevel.TryGetValue(level.ToString(), out var host) ? host : null;
}

public sealed class WebsiteHostLocationOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;

    /// <summary>對應 Provisioning:Servers 的 Id；沒有 Worker（例如 NAS封存）為 null。</summary>
    public string? ServerId { get; set; }

    public bool HasWorker => !string.IsNullOrWhiteSpace(ServerId);
}