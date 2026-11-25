using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Application.Shared.Templates;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using EtheriT.Coker.Web.Core.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EtheriT.Coker.Application.SiteCrawl;

internal sealed class SiteLayoutResult
{
    public int MenusCreated { get; set; }
    public int MenusUpdated { get; set; }
    public List<ImportedSiteMenu> ImportedMenus { get; } = [];
    public int ArticlesCreated { get; set; }
    public int ArticlesUpdated { get; set; }
    public int ArticlesFetched { get; set; }
    public bool ArticleLimitReached { get; set; }
    public int TagsCreated { get; set; }
    public int DirectoriesCreated { get; set; }
    public int ListPagesBound { get; set; }
    public int SectionMenus { get; set; }
    public int MarqueesImported { get; set; }
    public int ArticleTemplatesApplied { get; set; }
    public int PagesScanned { get; set; }
    public int PagesRendered { get; set; }
    public int PagesImported { get; set; }
    public int HiddenMenus { get; set; }
    public int FormsConverted { get; set; }
    public int StylesheetsImported { get; set; }
    public int PagesWithBanner { get; set; }
    public int PagesWithFooter { get; set; }
    public int BannerVotes { get; set; }
    public int FooterVotes { get; set; }
    public int SlidesAdded { get; set; }
    public bool FooterUpdated { get; set; }
    // 網站資料的網站Logo：imported＝已設定、unchanged＝和舊站相同、kept＝網站已有其他 Logo 沒覆蓋；null＝沒匯入。
    public string? LogoStatus { get; set; }
    public bool PageLimitReached { get; set; }
    public HashSet<string> QueryLimitPaths { get; } = new(StringComparer.Ordinal);
    // 確認畫面標成商品的列表裡的項目，建在商品管理的商品列表。
    public int ProductsCreated { get; set; }
    public int ProductsUpdated { get; set; }
    // 標成商品的列表（SiteListDetector.ListKey）。
    internal HashSet<string> ProductListKeys { get; } = new(StringComparer.Ordinal);
    // 確認畫面的主選單選擇（SiteMenuSelection.Key → 選擇）；空的＝全部匯入（舊任務或沒有選擇資料）。
    internal Dictionary<string, SiteMenuChoice> MenuChoices { get; } = new(StringComparer.Ordinal);
    // 使用者取消勾選的頁面（PageKey）：連同左欄子頁、文章列表、隱藏頁都不匯入，內文連到它時指回舊站。
    internal HashSet<string> ExcludedPageKeys { get; } = new(StringComparer.Ordinal);
    // 已匯入的主選單 Id → 確認畫面的 Key，匯入左欄子頁時用來找它們的勾選與路徑名稱。
    internal Dictionary<long, string> MenuSelectionKeys { get; } = [];
    public int MenusExcluded { get; set; }
    public List<string> Errors { get; } = [];
    public int ErrorCount { get; private set; }
    internal bool ScanComplete { get; set; }
    internal string? OrgName { get; set; }
    // 已改放目錄元件的選單（例 分類首頁改選單目錄），列表綁定與內容匯入都不再覆蓋。
    internal HashSet<long> HandledMenuIds { get; } = [];
    internal string? FirstPageDiagnostic { get; set; }
    // 目前正在清理的內容是哪裡（例「產品A」商品、「最新消息」頁面），內文附件匯入失敗時用來說明，不寫舊站路徑。
    internal string? ContentSource { get; set; }
    // 已匯入的文章／商品：舊站識別（主機|id 或 主機|sub:sub_id）→ 新站網址，內文、頁尾連到詳細頁時改寫用。
    internal Dictionary<string, string> DetailRoutes { get; } = new(StringComparer.Ordinal);
    // 讀主選單時入口頁跳轉經過的網址（例 / → default1.asp），都算舊站首頁。
    internal List<Uri> EntryRedirects { get; } = [];

    internal static string SourceName(string? title, Uri url, string kind) =>
        $"「{(string.IsNullOrWhiteSpace(title) ? url.PathAndQuery : title.Trim())}」{kind}";

    public void AddError(string message)
    {
        ErrorCount++;
        if (Errors.Count < 50) Errors.Add(message.Length > 350 ? message[..350] : message);
    }

    public string Serialize() => JsonSerializer.Serialize(this, JsonSerializerOptions.Web);
}

public sealed class SiteLayoutImporter(SiteLayoutScanner scanner, SiteCrawlHtml html,
    ITemplatesApplicationService templates, IHtmlSanitizer sanitizer,
    CokerDbContext db, LoginUserData user, ILogger<SiteLayoutImporter> logger)
{
    internal async Task<SiteLayoutSelection> ImportAsync(Uri root, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var pages = await scanner.ScanAsync(root, result, checkpoint, token);
        result.ScanComplete = true;
        if (pages.Count == 0) throw new InvalidOperationException("沒有抓取到可分析的 HTML 頁面。");
        var banner = SiteLayoutScanner.SelectLayout(pages, page => page.Banner);
        var footer = SiteLayoutScanner.SelectLayout(pages, page => page.Footer);
        result.BannerVotes = banner.Votes;
        result.FooterVotes = footer.Votes;
        await ImportPartAsync("網站Logo", () => ImportLogoAsync(pages, result, checkpoint, token), result, token);
        return new SiteLayoutSelection(pages, banner.Page, footer.Page);
    }

    // 版頭輪播與頁尾的連結要改寫成新站網址，所以等左欄子頁、隱藏選單（子分類）、文章、商品都建好後才匯入；
    // 之前在掃描完就匯入，頁尾連到子分類的連結全都留在舊站（2026-11-18 superiorlink 實測）。
    internal async Task ImportBannerAndFooterAsync(SiteLayoutSelection layout, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        await ImportPartAsync("網站版型", () => EnsureTemplateLayoutAsync(checkpoint, token), result, token);
        if (layout.BannerPage is { } bannerPage)
            await ImportPartAsync("版頭", () => ImportBannerAsync(bannerPage, result, checkpoint, token), result, token);
        else
            result.AddError("沒有辨識到可匯入的版頭。");
        if (layout.FooterPage is { } footerPage)
            await ImportPartAsync("頁尾", () => ImportFooterAsync(footerPage, result, checkpoint, token), result, token);
        else
            result.AddError("沒有辨識到可匯入的頁尾。");
    }

    // 前台只有版型 7、8 才讀樣板（TemplatesApplicationService.GetDefaultTemplatesAsync），
    // 版型空白時版頭頁尾會變成 Default_Header／Default_Footer，匯入的 Banner 與頁尾都不會顯示。
    // 只補空白的站，已設定其他版型的網站不動；正式機碰不到資料庫，重新匯入一次就能修好舊資料。
    private const int TemplateLayoutType = 7;

    private async Task EnsureTemplateLayoutAsync(Func<Task> checkpoint, CancellationToken token)
    {
        var websiteId = await user.GetWebsiteId();
        var website = await db.Websites.FirstOrDefaultAsync(x => x.Id == websiteId, token)
            ?? throw new InvalidOperationException("找不到目前網站。");
        if (website.LayoutType is not (null or 0)) return;
        await checkpoint();
        website.LayoutType = TemplateLayoutType;
        await user.SaveChanges(website);
    }

    // 各頁版頭的 Logo 取最多頁用的那張（同票取先掃到的），設成網站資料的網站Logo（使用者 2026-11-18 要求）。
    private async Task ImportLogoAsync(IReadOnlyList<SiteLayoutScanner.LayoutPage> pages, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var logo = pages.Where(page => page.Logo != null)
            .GroupBy(page => page.Logo!.AbsoluteUri, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(group => group.Count())
            .FirstOrDefault()?.First();
        if (logo == null)
        {
            result.AddError("沒有辨識到網站 Logo。");
            return;
        }
        result.LogoStatus = await html.ImportWebsiteLogoAsync(logo.Logo!, logo.Url, result, checkpoint, token);
    }

    private async Task ImportPartAsync(string part, Func<Task> import, SiteLayoutResult result, CancellationToken token)
    {
        try
        {
            await import();
        }
        catch (Exception ex) when (ex is not OperationCanceledException && !token.IsCancellationRequested)
        {
            logger.LogError(ex, "Site crawl layout import failed for {Part}", part);
            result.AddError($"{part}匯入失敗，請查看 MVC 日誌。");
        }
    }

    private async Task ImportBannerAsync(SiteLayoutScanner.LayoutPage page, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var images = new List<SliderDto>();
        var candidates = page.Banner!.DescendantsAndSelf()
            .Where(x => x.Name == "img" || x.GetAttributeValue("style", "").Contains("background-image", StringComparison.OrdinalIgnoreCase));
        foreach (var image in candidates.Take(50))
        {
            var desktop = await html.ImportImageAsync(SiteCrawlHtml.ImageSource(image), page.Url, result, checkpoint, token);
            if (desktop == null) continue;
            var mobileSource = image.ParentNode?.Name == "picture"
                ? image.ParentNode.Elements("source").FirstOrDefault(x =>
                    x.GetAttributeValue("media", "").Contains("max-width", StringComparison.OrdinalIgnoreCase))
                : null;
            var mobile = mobileSource == null ? null :
                await html.ImportImageAsync(SiteCrawlHtml.ImageSource(mobileSource), page.Url, result, checkpoint, token);
            images.Add(new SliderDto
            {
                DesktopImage = desktop,
                MobileImage = mobile,
                Link = ResolveBannerLink(image, page.Url, result),
                Enabled = true
            });
        }
        if (images.Count == 0)
        {
            result.AddError("已辨識版頭，但沒有可匯入的輪播圖片。");
            return;
        }
        await checkpoint();
        var response = await templates.getDefaultHeader();
        if (!response.Success || response.Object is not HeaderTemplateDto header)
            throw new InvalidOperationException("無法讀取現有版頭。");
        var sliders = header.ContentConfig.Sliders;
        var existing = sliders.SelectMany(x => new[] { x.DesktopImage, x.MobileImage }).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var added = images.Where(x => existing.Add(x.DesktopImage)).ToList();
        var linksChanged = false;
        foreach (var image in images.Where(x => x.Link != null))
        {
            foreach (var slider in sliders.Where(x =>
                string.Equals(x.DesktopImage, image.DesktopImage, StringComparison.OrdinalIgnoreCase) && x.Link != image.Link))
            {
                slider.Link = image.Link;
                linksChanged = true;
            }
        }
        if (added.Count == 0 && !linksChanged) return;
        sliders.AddRange(added);
        await checkpoint();
        if (!(await templates.saveDefaultHeader(header)).Success)
            throw new InvalidOperationException("無法儲存版頭。");
        result.SlidesAdded = added.Count;
    }

    private static string? ResolveBannerLink(HtmlNode image, Uri pageUrl, SiteLayoutResult result)
    {
        if (image.ParentNode?.Name != "a") return null;
        var href = HtmlEntity.DeEntitize(image.ParentNode.GetAttributeValue("href", "")).Trim();
        if (href.Length == 0 || href.StartsWith('#') || !Uri.TryCreate(pageUrl, href, out var target)
            || target.Scheme is not ("http" or "https"))
            return null;

        return SiteCrawlHtml.FindRoute(result, target) ?? target.AbsoluteUri;
    }

    private async Task ImportFooterAsync(SiteLayoutScanner.LayoutPage page, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var node = page.Footer!.CloneNode(true);
        result.ContentSource = "網站頁尾";
        var css = await html.CleanAsync(node, page.Url, result, allowIframes: false, checkpoint, token);
        await checkpoint();
        var current = await templates.GetDefaultFooterTemplatesAsync();
        if (!current.Success || current.Object is not TemplateSectionsDto section || section.footerTemplateDto?.id is not long footerId)
            throw new InvalidOperationException("無法讀取現有頁尾。");
        var cleaned = sanitizer.SanitizePublicHtml(node.OuterHtml);
        if (string.IsNullOrWhiteSpace(cleaned))
            throw new InvalidOperationException("沒有可儲存的頁尾內容。");
        var input = new MenuSaveContenDto { Id = checked((int)footerId), SaveHtml = cleaned, SaveCss = sanitizer.SanitizePublicCss(css) };
        if (!(await templates.importDefaultFooter(input)).Success)
            throw new InvalidOperationException("無法儲存頁尾。");
        result.FooterUpdated = true;
    }
}
