namespace EtheriT.Coker.Web.Platform.Models.Websites;

/// <summary>Platform 可選的站台語系；值要與 Website.Locale 一致（小寫文化代碼）。</summary>
public static class WebsiteLocales
{
    public static readonly IReadOnlyList<string> All = ["zh-tw", "en"];

    /// <summary>去空白、轉小寫；空值或不在清單內回傳 null。</summary>
    public static string? Normalize(string? value)
    {
        var candidate = value?.Trim().ToLowerInvariant();
        return candidate is not null && All.Contains(candidate) ? candidate : null;
    }
}