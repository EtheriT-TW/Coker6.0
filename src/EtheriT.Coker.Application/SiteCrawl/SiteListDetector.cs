using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Microsoft.Extensions.Options;

namespace EtheriT.Coker.Application.SiteCrawl;

// Product＝預猜（或鎖定）匯入成商品；Locked＝舊系統購物商品，不給改。
public sealed record SiteListCandidate(string Key, string Url, string Title, string Path, string KindName,
    bool Product, bool Locked, string Reason);

public sealed class SiteDetectResult
{
    public string Mode => "detect";
    public string SourceUrl { get; set; } = "";
    public int PagesScanned { get; set; }
    public bool PageLimitReached { get; set; }
    public List<SiteListCandidate> Lists { get; } = [];
    // 舊站主選單（和匯入時同一套讀法），給確認畫面勾選要匯入的頁面。
    public List<SiteMenuCandidate> Menus { get; } = [];
    public List<string> Errors { get; } = [];
    public int ErrorCount { get; private set; }

    public void AddError(string message)
    {
        ErrorCount++;
        if (Errors.Count < 20) Errors.Add(message.Length > 350 ? message[..350] : message);
    }

    public string Serialize() => JsonSerializer.Serialize(this, JsonSerializerOptions.Web);
}

// 匯入前的列表偵測（使用者 2026-11-17 決定：先猜文章／商品，再讓使用者確認）。
// 舊站列表由 loadImg.js 的 new getImgList({ prodSalesType:N }) 用 JS 載入，但類型寫在頁面原始碼裡，
// 所以只用一般 HTTP、不開瀏覽器；詳細頁（網址帶 id／prod_id）不是列表，不抓。
// prodSalesType 對照（loadImg.js）：0、1～12、100、101、111 和未知值都是舊系統購物商品；
// 102 新聞、103 型錄、104 下載、105 影片、109 門市、110 行程；106 全部、107 相簿、108 子分類、200 搜尋不是單篇內容的列表。
// 型錄（103）在舊系統裡和文章同一種，HTML 分不出來（2026-11-17 sf2、superiorlink、ayawan 實測），只能用名稱猜。
public sealed partial class SiteListDetector(SiteCrawlHttp http, IOptions<SiteCrawlOptions> options)
{
    private const int MaxRedirects = 5;
    private const int MaxTitleLength = 100;
    private const string PageParameter = "page";
    private static readonly TimeSpan FetchDelay = TimeSpan.FromMilliseconds(200);
    private static readonly string[] DetailParameters = ["id", "prod_id"];
    private static readonly string[] ProductWords = ["產品", "商品", "product"];
    private static readonly HashSet<int> NotContentLists = [106, 107, 108, 200];
    private static readonly Dictionary<int, string> ContentLists = new()
    {
        [102] = "新聞", [103] = "型錄", [104] = "下載", [105] = "影片", [109] = "門市", [110] = "行程"
    };
    private const string NewsLinks = "//*[@id='cont']//a[@href][contains(concat(' ',normalize-space(@class),' '),' newsItem ')]";
    // 伺服器直接輸出的購物商品卡片（一般是 JS 載入，這是備案）；內文裡偶爾連到單一商品的連結不算。
    private const string ProductLinks =
        "//*[@id='cont']//a[contains(concat(' ',normalize-space(@class),' '),' prod_link ')][contains(@href,'prod_id=')]";

    [GeneratedRegex(@"new\s+getImgList\s*\(\s*\{[^}]*?prodSalesType\s*:\s*[""']?(?<type>\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex ListScript();

    [GeneratedRegex(@"\.(pdf|jpe?g|png|gif|webp|bmp|svg|zip|rar|7z|docx?|xlsx?|pptx?|mp4|mp3|avi|mov)$", RegexOptions.IgnoreCase)]
    private static partial Regex FileExtension();

    // 連到檔案（圖片、PDF…）的連結不是網頁，不抓：舊站對帶 Accept: text/html 的圖片請求回 406（sf2 實測）。
    internal static bool IsFileLink(Uri uri) => FileExtension().IsMatch(uri.AbsolutePath);

    internal async Task DetectAsync(Uri root, SiteDetectResult result, Func<int, Task> checkpoint, CancellationToken token)
    {
        result.SourceUrl = root.AbsoluteUri;
        var host = root.IdnHost;
        var pending = new Queue<Uri>([SiteLayoutScanner.Normalize(root)]);
        var seen = new HashSet<string>(StringComparer.Ordinal) { SiteLayoutScanner.PageKey(root) };
        var found = new Dictionary<string, SiteListCandidate>(StringComparer.Ordinal);
        var redirects = 0;
        while (pending.TryDequeue(out var uri))
        {
            await checkpoint(result.PagesScanned);
            try
            {
                var reply = await http.GetAsync(uri, host, false, 2 * 1024 * 1024, token);
                await using var content = reply.Content;
                var document = SiteLayoutScanner.ReadDocument(content, reply.Charset);
                if (SiteLayoutScanner.FindEntryTarget(document, reply.Url) is { } target)
                {
                    if (++redirects <= MaxRedirects) Enqueue(target, host, pending, seen, result);
                    continue;
                }
                result.PagesScanned++;
                var url = SiteLayoutScanner.Normalize(reply.Url);
                if (Classify(document, url, host) is { } candidate) found.TryAdd(candidate.Key, candidate);
                foreach (var link in document.DocumentNode.SelectNodes("//a[@href]") ?? Enumerable.Empty<HtmlNode>())
                    if (Uri.TryCreate(reply.Url, HtmlEntity.DeEntitize(link.GetAttributeValue("href", "")).Trim(), out var next))
                        Enqueue(next, host, pending, seen, result);
            }
            catch (HttpRequestException) when (!token.IsCancellationRequested)
            {
                // 舊站自己的失效連結或暫時錯誤；正式匯入時會再抓一次並列出。
            }
            catch (InvalidOperationException) when (!token.IsCancellationRequested)
            {
                // 非 HTML（檔案下載）或超出網域的轉址，不是列表頁。
            }
            if (pending.Count > 0) await Task.Delay(FetchDelay, token);
        }
        result.Lists.AddRange(found.Values.OrderBy(item => item.Path, StringComparer.Ordinal)
            .ThenBy(item => item.Title, StringComparer.Ordinal));
        if (result.PagesScanned == 0) result.AddError("沒有讀到任何舊站頁面，請確認網址。");
    }

    private void Enqueue(Uri uri, string host, Queue<Uri> pending, HashSet<string> seen, SiteDetectResult result)
    {
        if (!SiteCrawlHttp.IsAllowed(uri, host) || IsFileLink(uri)) return;
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (DetailParameters.Any(name => !string.IsNullOrWhiteSpace(query[name]))) return;
        var normalized = SiteLayoutScanner.Normalize(uri);
        if (!seen.Add(SiteLayoutScanner.PageKey(normalized))) return;
        if (options.Value.MaxPages > 0 && seen.Count > options.Value.MaxPages)
        {
            result.PageLimitReached = true;
            return;
        }
        pending.Enqueue(normalized);
    }

    private static SiteListCandidate? Classify(HtmlDocument document, Uri url, string host)
    {
        var kind = ListKind(document);
        if (kind == null) return null;
        var title = Truncate(SiteArticleDetector.ReadSubTitle(document) ?? SiteArticleDetector.PageName(document)
            ?? url.PathAndQuery, MaxTitleLength);
        var path = string.Join(" > ", SiteArticleDetector.ReadBreadcrumb(document, url, host));
        var key = ListKey(url);
        if (kind.Value.Shop)
            return new SiteListCandidate(key, url.AbsoluteUri, title, path, kind.Value.Name, true, true,
                $"舊系統購物商品（{kind.Value.Name}）");
        if (kind.Value.Name != ContentLists[103])
            return new SiteListCandidate(key, url.AbsoluteUri, title, path, kind.Value.Name, false, false,
                $"{kind.Value.Name}列表，預設文章");
        var word = ProductWords.FirstOrDefault(item => title.Contains(item, StringComparison.OrdinalIgnoreCase)
            || path.Contains(item, StringComparison.OrdinalIgnoreCase));
        return word == null
            ? new SiteListCandidate(key, url.AbsoluteUri, title, path, kind.Value.Name, false, false, "名稱沒有「產品／商品」")
            : new SiteListCandidate(key, url.AbsoluteUri, title, path, kind.Value.Name, true, false, $"名稱含「{word}」");
    }

    private static (string Name, bool Shop)? ListKind(HtmlDocument document)
    {
        foreach (var script in document.DocumentNode.SelectNodes("//script") ?? Enumerable.Empty<HtmlNode>())
        {
            var match = ListScript().Match(script.InnerHtml);
            if (!match.Success || !int.TryParse(match.Groups["type"].Value, out var type)) continue;
            if (NotContentLists.Contains(type)) return null;
            return ContentLists.TryGetValue(type, out var name) ? (name, false) : ($"購物商品 {type}", true);
        }
        if (document.DocumentNode.SelectSingleNode(ProductLinks) != null) return ("購物商品", true);
        return document.DocumentNode.SelectSingleNode(NewsLinks) != null ? (ContentLists[102], false) : null;
    }

    // 列表的識別：同一個列表的分頁（例 superiorlink 訊息公告 Page=2、Page=3）視為同一個，偵測與匯入都用這個比對。
    internal static string ListKey(Uri url)
    {
        var query = url.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Where(item => !Uri.UnescapeDataString(item.Split('=', 2)[0]).Equals(PageParameter, StringComparison.OrdinalIgnoreCase));
        return SiteLayoutScanner.PageKey(new UriBuilder(url) { Query = string.Join("&", query) }.Uri);
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];
}
