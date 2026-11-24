using System.Text.RegularExpressions;
using System.Web;

namespace EtheriT.Coker.Application.ArticleImports;

// Google 表單上傳的檔案在 Excel 裡是 https://drive.google.com/open?id=檔案ID（多個檔案用逗號分隔）。
public static class ArticleImportDriveLinks
{
    private static readonly Regex UrlPattern = new(
        @"https?://(?:drive|docs)\.google\.com/[^\s,，、;；""'<>]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex IdPattern = new(@"^[A-Za-z0-9_-]{10,200}$", RegexOptions.Compiled);
    private static readonly Regex PathIdPattern = new(@"/d/([A-Za-z0-9_-]{10,200})(?:/|$)", RegexOptions.Compiled);

    // 文字裡所有可下載的 Drive 連結（同一個檔案只回傳一次）。
    public static IReadOnlyList<string> FindFileIds(string text)
        => UrlPattern.Matches(text)
            .Select(m => ExtractFileId(m.Value))
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

    // 把文字裡的 Drive 連結換成 replace(檔案ID) 的結果；不是檔案連結（例如資料夾）的保留原文。
    public static string Replace(string text, Func<string, string> replace)
        => UrlPattern.Replace(text, m => ExtractFileId(m.Value) is { } id ? replace(id) : m.Value);

    public static string? ExtractFileId(string url)
    {
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)
            || uri.Host is not ("drive.google.com" or "docs.google.com"))
            return null;

        var id = HttpUtility.ParseQueryString(uri.Query)["id"];
        if (string.IsNullOrEmpty(id))
        {
            var match = PathIdPattern.Match(uri.AbsolutePath);
            id = match.Success ? match.Groups[1].Value : null;
        }
        return id != null && IdPattern.IsMatch(id) ? id : null;
    }

    // 下載網址固定由檔案 ID 組出來，不直接用 Excel 裡的網址。
    public static Uri DownloadUrl(string fileId)
    {
        if (!IdPattern.IsMatch(fileId)) throw new ArgumentException("Google Drive 檔案 ID 不正確。", nameof(fileId));
        return new Uri($"https://drive.google.com/uc?export=download&id={fileId}");
    }

    // drive.google.com 的下載網址會轉址到 drive.usercontent.google.com（舊檔案是 *.googleusercontent.com），
    // 這些是 Google 存放檔案的網域，也要允許；其他網域一律拒絕。
    public static bool IsAllowedHost(Uri uri)
    {
        if (!uri.IsAbsoluteUri || uri.Scheme != Uri.UriSchemeHttps || uri.Port != 443 || uri.UserInfo.Length > 0)
            return false;
        var host = uri.IdnHost.ToLowerInvariant();
        return host is "drive.google.com" or "docs.google.com" or "drive.usercontent.google.com"
            || host.EndsWith(".googleusercontent.com", StringComparison.Ordinal);
    }
}