using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
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
    public int Unchanged { get; private set; }
    public int Conflicts { get; private set; }
    public List<string> UnchangedMenuTitles { get; } = [];
    public List<string> ConflictTitles { get; } = [];
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
    internal SiteCrawlImportState? ImportState { get; set; }
    internal SiteCrawlPendingStore? PendingStore { get; set; }
    internal long ImportTaskId { get; set; }
    public int? Removed { get; internal set; }
    public int? NotScanned { get; internal set; }
    internal HashSet<Uri> TimedOutSources { get; } = [];
    internal HashSet<string> SeenSliderKeys { get; } = new(StringComparer.Ordinal);
    internal bool SliderSectionSeen { get; set; }
    internal HashSet<string> SeenMarqueeKeys { get; } = new(StringComparer.Ordinal);
    internal HashSet<(string Kind, long Id)> ClaimedRemovalTargets { get; } = [];
    internal HashSet<(long HeaderId, string DesktopImage)> ClaimedSliders { get; } = [];
    internal bool IsFirstImport { get; set; }
    internal HashSet<long> CreatedMenuIds { get; } = [];
    internal bool DirectoryBindingsChanged { get; set; }
    private bool importStateWriteFailed;
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

    internal void CountUnchanged() => Unchanged++;

    internal bool CheckContentUnchanged(SiteCrawlContentSnapshot before, SiteCrawlContentSnapshot? after, string title)
    {
        // 下載圖片和整理內容需要時間，寫入前再讀一次，避免這段期間的手動編輯被蓋掉。
        if (before == after) return true;
        Conflicts++;
        if (ConflictTitles.Count < 20) ConflictTitles.Add(title);
        return false;
    }

    internal SiteCrawlImportDecision DecideImport(SiteCrawlStateItem? previous, string hash,
        SiteCrawlContentSnapshot current, string title, bool isMenu = false)
    {
        var decision = SiteCrawlReimport.Decide(hash, previous?.SourceHash, previous?.WrittenHash,
            current.Html, current.Css, previous == null && IsFirstImport, current.SaveHtml, current.SaveCss,
            previous?.WrittenDraftHash,
            // 舊對照無法辨識寫入指紋是否曾被衝突處理改寫。
            requiresManualReview: previous is { WrittenHash: not null, HasVerifiedContentBaseline: false });
        if (decision == SiteCrawlImportDecision.Unchanged)
        {
            CountUnchanged();
            if (isMenu && UnchangedMenuTitles.Count < 20) UnchangedMenuTitles.Add(title);
        }
        if (decision == SiteCrawlImportDecision.Conflict)
        {
            Conflicts++;
            if (ConflictTitles.Count < 20) ConflictTitles.Add(title);
        }
        return decision;
    }

    internal async Task StageConflictAsync(SiteCrawlConflictKind kind, long id, string key, Uri source,
        string sourceHash, string title, SiteCrawlContentSnapshot current, SiteCrawlStateItem? previous,
        string html, string css, string? importedTitle = null, bool hideGlobalFooter = false)
    {
        if (PendingStore == null || OrgName == null || ImportState == null) return;
        var hash = SiteCrawlFingerprint.ComputeWritten(current.Html, current.Css);
        var draftHash = SiteCrawlFingerprint.ComputeWritten(current.SaveHtml, current.SaveCss);
        var rename = importedTitle != null && previous?.WrittenTitle != null && title != previous.SourceTitle;
        var newUrl = kind switch
        {
            SiteCrawlConflictKind.Footer => $"/{Uri.EscapeDataString(OrgName)}/",
            SiteCrawlConflictKind.Menu => SiteCrawlHtml.FindMenuRoute(this, source),
            _ => SiteCrawlHtml.FindDetailRoute(this, source)
        };
        await PendingStore.WriteAsync(OrgName, new(ImportTaskId, kind, id, key, source.AbsoluteUri,
            newUrl, title, sourceHash, html, css, hash, draftHash,
            rename ? previous!.WrittenTitle : null, rename ? importedTitle : null, hideGlobalFooter));
        ImportState.RememberConflict(key, id, source, title, sourceHash);
    }

    internal async Task RecordImportTargetAsync(string key, long id, Uri? source, string? title, bool created,
        string? writtenTitle = null, SiteCrawlFields? writtenFields = null)
    {
        if (ImportState == null) return;
        try
        {
            await ImportState.RecordTargetAsync(key, id, source, title, created, writtenTitle, writtenFields);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ReportImportStateWriteFailure();
        }
    }

    internal async Task SaveImportStateAsync()
    {
        if (ImportState == null) return;
        try
        {
            await ImportState.SaveAsync();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ReportImportStateWriteFailure();
        }
    }

    private void ReportImportStateWriteFailure()
    {
        if (importStateWriteFailed) return;
        importStateWriteFailed = true;
        AddError("網站匯入對照未能儲存，請確認站台 Upload 目錄可寫入。");
    }
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
        var images = page.Banner!.DescendantsAndSelf().Where(IsBannerImage).ToList();
        var candidates = images.Take(50).ToList();
        if (candidates.Count == 0)
        {
            result.AddError("已辨識版頭，但沒有可匯入的輪播圖片。");
            return;
        }

        // 以輪播區讀到的來源網址判斷是否存在，下載圖片失敗不代表舊站移除了輪播。
        foreach (var image in images)
        {
            var rawImage = SiteCrawlHtml.ImageSource(image);
            if (BannerImageUrl(page.Url, rawImage) is { } sourceUrl)
            {
                result.SeenSliderKeys.Add("Slider|" + sourceUrl.AbsoluteUri);
                result.SliderSectionSeen = true;
            }
            else if (rawImage.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                result.SliderSectionSeen = true;
            }
        }
        await checkpoint();
        var response = await templates.getDefaultHeader();
        if (!response.Success || response.Object is not HeaderTemplateDto header)
            throw new InvalidOperationException("無法讀取現有版頭。");
        var sliders = header.ContentConfig.Sliders;
        var headerId = result.ImportState == null ? null : (await ReadHeaderAsync())?.Id;
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var paths = sliders.SelectMany(x => new[] { x.DesktopImage, x.MobileImage }).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var observed = new List<(Uri Source, string Hash, SliderDto Slider, string? WrittenLinkHash)>();
        var added = 0;
        var linksChanged = false;
        var found = false;

        foreach (var image in candidates)
        {
            var raw = SiteCrawlHtml.ImageSource(image);
            var source = BannerImageUrl(page.Url, raw);
            var key = source == null ? null : "Slider|" + source.AbsoluteUri;
            if (key != null && !seen.Add(key)) continue;
            var previous = key == null ? null : result.ImportState?.Find(key);
            var mobileSource = image.ParentNode?.Name == "picture"
                ? image.ParentNode.Elements("source").FirstOrDefault(x =>
                    x.GetAttributeValue("media", "").Contains("max-width", StringComparison.OrdinalIgnoreCase))
                : null;
            var mobileRaw = mobileSource == null ? null : SiteCrawlHtml.ImageSource(mobileSource);
            var rawLink = image.ParentNode?.Name == "a" ? image.ParentNode.GetAttributeValue("href", "") : null;
            var hash = SiteCrawlFingerprint.ComputeWritten(rawLink, mobileRaw);
            var link = ResolveBannerLink(image, page.Url, result);
            var slider = previous?.NewId == headerId && previous?.DesktopImage is { } desktopPath
                ? sliders.FirstOrDefault(x => SameDesktop(x, desktopPath, result.OrgName)) : null;

            if (slider != null && previous?.SourceHash == hash)
            {
                result.CountUnchanged();
                found = true;
                continue;
            }
            if (slider == null)
            {
                var desktop = await html.ImportImageAsync(raw, page.Url, result, checkpoint, token);
                if (desktop == null) continue;
                slider = sliders.FirstOrDefault(x => SameDesktop(x, PublishedDesktop(desktop, result.OrgName), result.OrgName));
                if (slider == null && paths.Add(desktop))
                {
                    var mobile = mobileRaw == null ? null :
                        await html.ImportImageAsync(mobileRaw, page.Url, result, checkpoint, token);
                    slider = new SliderDto { DesktopImage = desktop, MobileImage = mobile, Link = link, Enabled = true };
                    sliders.Add(slider);
                    added++;
                }
                found = true;
                if (slider == null) continue;
            }

            found = true;
            var writtenLinkHash = previous?.WrittenHash;
            if (slider.Link != link && ((previous == null && result.IsFirstImport && link != null)
                || writtenLinkHash != null && writtenLinkHash == SiteCrawlFingerprint.ComputeWritten(slider.Link, null)))
            {
                slider.Link = link;
                linksChanged = true;
            }
            // 未曾記錄過連結時，只有目前連結符合來源才可作為日後更新的依據。
            if (slider.Link == link) writtenLinkHash = SiteCrawlFingerprint.ComputeWritten(slider.Link, null);
            if (source != null) observed.Add((source, hash, slider, writtenLinkHash));
        }

        if (!found)
        {
            result.AddError("已辨識版頭，但沒有可匯入的輪播圖片。");
            return;
        }
        if (added > 0 || linksChanged)
        {
            await checkpoint();
            if (!(await templates.saveDefaultHeader(header)).Success)
                throw new InvalidOperationException("無法儲存版頭。");
            result.SlidesAdded = added;
        }
        if (result.ImportState is not { } state || observed.Count == 0
            || await ReadHeaderAsync() is not { } savedHeader || string.IsNullOrWhiteSpace(savedHeader.ContentConfig)) return;
        var saved = Newtonsoft.Json.JsonConvert.DeserializeObject<HeaderContentConfigDto>(savedHeader.ContentConfig)?.Sliders;
        if (saved == null) return;
        foreach (var (source, hash, slider, writtenLinkHash) in observed)
        {
            var desktop = PublishedDesktop(slider.DesktopImage, result.OrgName);
            var match = saved.FirstOrDefault(x => string.Equals(x.DesktopImage, desktop, StringComparison.OrdinalIgnoreCase));
            if (match == null || string.IsNullOrEmpty(match.DesktopImage)) continue;
            result.ClaimedSliders.Add((savedHeader.Id, match.DesktopImage.ToUpperInvariant()));
            state.RecordWritten("Slider|" + source.AbsoluteUri, savedHeader.Id, source, null, hash,
                new(writtenLinkHash, match.Title), desktopImage: match.DesktopImage);
        }
        if (added > 0) await result.SaveImportStateAsync();
    }

    private async Task<HeaderSnapshot?> ReadHeaderAsync()
    {
        var websiteId = await user.GetWebsiteId();
        var templateId = await db.Templates.AsNoTracking()
            .Where(x => x.FK_WebsiteID == websiteId && !x.IsDeleted && x.Enable)
            .OrderByDescending(x => x.LastModificationTime ?? x.CreationTime)
            .Select(x => x.Id).FirstOrDefaultAsync();
        return await db.TemplateSections.AsNoTracking()
            .Where(x => x.FK_TemplateID == templateId && x.template.FK_WebsiteID == websiteId
                && !x.IsDeleted && x.sectionType == SectionTypeEnum.表頭)
            .Select(x => new HeaderSnapshot(x.Id, x.ContentConfig)).FirstOrDefaultAsync();
    }

    private sealed record HeaderSnapshot(long Id, string ContentConfig);

    private static bool IsBannerImage(HtmlNode node) =>
        node.Name == "img" || node.GetAttributeValue("style", "").Contains("background-image", StringComparison.OrdinalIgnoreCase);

    private static Uri? BannerImageUrl(Uri page, string raw) =>
        !string.IsNullOrWhiteSpace(raw) && Uri.TryCreate(page, HtmlEntity.DeEntitize(raw).Trim(), out var url)
            && url.Scheme is ("http" or "https") ? new UriBuilder(url) { Fragment = "" }.Uri : null;

    private static string? PublishedDesktop(string? path, string? orgName) =>
        path?.Replace($"/upload/{orgName}/", "/upload/");

    private static bool SameDesktop(SliderDto slider, string? desktop, string? orgName) =>
        string.Equals(PublishedDesktop(slider.DesktopImage, orgName), desktop, StringComparison.OrdinalIgnoreCase);

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
        var key = "Footer|" + SiteCrawlStateStore.NormalizeHost(page.Url.IdnHost);
        var hash = SiteCrawlFingerprint.ComputeSource(page.Footer, null);
        var previous = result.ImportState?.Find(key);
        var websiteId = await user.GetWebsiteId();
        var footers = db.FooterTemplates.AsNoTracking()
            .Where(x => !x.IsDeleted && !x.templateSections.IsDeleted && !x.templateSections.template.IsDeleted
                && x.templateSections.template.FK_WebsiteID == websiteId);
        var existing = previous == null ? null : await footers.SingleOrDefaultAsync(x => x.Id == previous.NewId, token);
        if (existing == null)
        {
            var current = await templates.GetDefaultFooterTemplatesAsync();
            if (!current.Success || current.Object is not TemplateSectionsDto section || section.footerTemplateDto?.id is not long id)
                throw new InvalidOperationException("無法讀取現有頁尾。");
            existing = await footers.SingleOrDefaultAsync(x => x.Id == id, token)
                ?? throw new InvalidOperationException("無法讀取現有頁尾。");
        }
        if (previous?.NewId != existing.Id) previous = null;
        var currentHtml = previous?.WrittenHash != null || !string.IsNullOrWhiteSpace(existing.html) ? existing.html : existing.saveHtml;
        var currentCss = previous?.WrittenHash != null || !string.IsNullOrWhiteSpace(existing.css) ? existing.css : existing.saveCss;
        var before = new SiteCrawlContentSnapshot(existing.html, existing.css, existing.saveHtml, existing.saveCss);
        var decision = result.DecideImport(previous, hash, before, "網站頁尾");
        if (decision == SiteCrawlImportDecision.Unchanged) return;

        var node = page.Footer!.CloneNode(true);
        result.ContentSource = "網站頁尾";
        var css = await html.CleanAsync(node, page.Url, result, allowIframes: false, checkpoint, token);
        await checkpoint();
        var cleaned = sanitizer.SanitizePublicHtml(node.OuterHtml);
        if (string.IsNullOrWhiteSpace(cleaned))
            throw new InvalidOperationException("沒有可儲存的頁尾內容。");
        var footerId = existing.Id;
        var latest = await footers.Where(x => x.Id == footerId)
            .Select(x => new SiteCrawlContentSnapshot(x.html, x.css, x.saveHtml, x.saveCss)).SingleOrDefaultAsync(token);
        var input = new MenuSaveContenDto { Id = checked((int)footerId), SaveHtml = cleaned, SaveCss = sanitizer.SanitizePublicCss(css) };
        if (decision == SiteCrawlImportDecision.Conflict || !result.CheckContentUnchanged(before, latest, "網站頁尾"))
        {
            if (latest != null)
                await result.StageConflictAsync(SiteCrawlConflictKind.Footer, footerId, key, page.Url, hash,
                    "網站頁尾", latest, previous, cleaned, input.SaveCss ?? "");
            return;
        }
        // 已有非空頁尾要等寫入成功才建立對照，避免失敗後誤判為可直接續寫。
        if (previous == null && string.IsNullOrWhiteSpace(currentHtml) && string.IsNullOrWhiteSpace(currentCss))
            await result.RecordImportTargetAsync(key, footerId, page.Url, null, created: true);
        if (!(await templates.importDefaultFooter(input)).Success)
            throw new InvalidOperationException("無法儲存頁尾。");
        result.FooterUpdated = true;
        if (result.ImportState is not { } state) return;
        var saved = await footers.Where(x => x.Id == footerId)
            .Select(x => new { x.html, x.css, x.saveHtml, x.saveCss }).SingleOrDefaultAsync();
        if (saved != null)
            state.RecordWritten(key, footerId, page.Url, null, hash,
                new(SiteCrawlFingerprint.ComputeWritten(saved.html, saved.css), null,
                    DraftHash: SiteCrawlFingerprint.ComputeWritten(saved.saveHtml, saved.saveCss)));
    }
}
