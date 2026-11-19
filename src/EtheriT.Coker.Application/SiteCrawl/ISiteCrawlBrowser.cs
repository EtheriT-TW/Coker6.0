namespace EtheriT.Coker.Application.SiteCrawl;

// 實作放在 Web.MVC：Application 前台也會引用，不能讓每個前台站台都帶著瀏覽器驅動。
public interface ISiteCrawlBrowser : IAsyncDisposable
{
    // scrollToEnd：列表是捲動才載入下一批時（例如舊站職缺），一直捲到不再出現新連結為止。
    Task<string> RenderAsync(Uri uri, bool scrollToEnd, CancellationToken token);
}

public interface ISiteCrawlBrowserFactory
{
    Task<ISiteCrawlBrowser> OpenAsync(string host, CancellationToken token);
}
