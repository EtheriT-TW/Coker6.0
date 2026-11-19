namespace EtheriT.Coker.Application.SiteCrawl;

public sealed class SiteCrawlOptions
{
    public int MaxPages { get; set; }
    // 文章篇數上限（含掃描時已抓到的），0 = 不限；列表項目的詳細頁不算在 MaxPages 內，測試時可用這個縮短時間。
    public int MaxArticles { get; set; }
    public int MaxQueryVariantsPerPath { get; set; }
    public int MaxSeconds { get; set; }
    // msedge 用主機已安裝的 Edge；主機沒有 Edge 時改成 chromium，並安裝 Playwright 的瀏覽器。
    public string BrowserChannel { get; set; } = "msedge";
}
