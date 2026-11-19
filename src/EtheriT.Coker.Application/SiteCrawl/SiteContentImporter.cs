using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType.WebMenu;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.SiteCrawl;

internal sealed record SiteLayoutSelection(
    IReadOnlyList<SiteLayoutScanner.LayoutPage> Pages,
    SiteLayoutScanner.LayoutPage? BannerPage,
    SiteLayoutScanner.LayoutPage? FooterPage);

public sealed class SiteContentImporter(SiteCrawlHtml html, SiteGalleryBuilder gallery, IWebMenuApplication webMenu, IHtmlSanitizer sanitizer,
    CokerDbContext db, LoginUserData user, ILogger<SiteContentImporter> logger)
{
    // 不能用 VisibleHeader=false：前台會把整個 <header> 連主選單一起藏掉。
    private const string HideGlobalBanner = "header #banner,header .image-wh,header .full-banner{display:none !important;}";
    private static readonly string[] NoiseClasses = ["hide", "popularity", "loader"];
    private const string LoadingStateClass = "opacity";
    private const string LoadingStateSelector =
        "descendant-or-self::*[contains(concat(' ',normalize-space(@class),' '),' " + LoadingStateClass + " ')]";
    // hd_marquee：首頁沒有 #cont、內文是整個 body，跑馬燈已匯入成全站跑馬燈，內文不再放一份。
    private static readonly string[] NoiseIds = ["list_ctrl", "pageBlock", "titleBlock", "hd_marquee"];
    private const string HiddenClass = "hide";
    // 舊站首頁整頁包在 #wrap.hide 裡、等 JS 載完才拿掉 hide（superiorlink default1.asp）：包著版頭或頁尾的 .hide 是載入遮罩，不是隱藏內容。
    private const string LoadingCover =
        ".//*[contains(concat(' ',normalize-space(@class),' '),' " + HiddenClass + " ')][.//header or .//footer or .//*[@id='bannerCont' or @id='foot']]";
    // 舊站 Bootstrap 2 格線：.row／.row-fluid 底下的 .spanN。
    private const string LegacySpan = "^span(?<size>[1-9]|1[0-2])$";
    private const string PageChrome = ".//header|.//nav|.//footer|.//aside|.//*[@role='navigation' or @role='banner' or @role='contentinfo']";

    internal async Task ImportAsync(SiteLayoutSelection layout, IReadOnlySet<long> handledMenuIds, SiteLayoutResult result,
        Func<Task> checkpoint, CancellationToken token)
    {
        var menus = result.ImportedMenus
            .Where(SiteLayoutScanner.CanHoldContent)
            .GroupBy(menu => SiteLayoutScanner.PageKey(new Uri(menu.SourceUrl!)))
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var websiteId = await user.GetWebsiteId();
        var recipient = await FormRecipientAsync(websiteId, token);
        var matched = new HashSet<string>(StringComparer.Ordinal);
        foreach (var page in layout.Pages.Where(page => page.Content != null))
        {
            await checkpoint();
            var key = SiteLayoutScanner.PageKey(page.Url);
            if (!menus.TryGetValue(key, out var menu) || !matched.Add(key)) continue;
            if (handledMenuIds.Contains(menu.Id)) continue; // 列表頁已改用文章匯入目錄元件
            try
            {
                await ImportPageAsync(page, menu, layout, websiteId, recipient, result, checkpoint, token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException && !token.IsCancellationRequested)
            {
                logger.LogError(ex, "Site crawl content import failed for menu {MenuId}", menu.Id);
                result.AddError($"選單「{menu.Title}」的內容匯入失敗，請查看 MVC 日誌。");
            }
        }
        foreach (var (key, menu) in menus)
            if (!matched.Contains(key))
                result.AddError($"選單「{menu.Title}」沒有抓到對應的頁面內容。");
    }

    private async Task ImportPageAsync(SiteLayoutScanner.LayoutPage page, ImportedSiteMenu menu, SiteLayoutSelection layout,
        long websiteId, SiteFormRecipient recipient, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var ownBanner = layout.BannerPage != null && !SiteLayoutScanner.IsSameSection(page.Banner, page.Url,
            layout.BannerPage.Banner, layout.BannerPage.Url);
        var ownFooter = layout.FooterPage != null && !SiteLayoutScanner.IsSameSection(page.Footer, page.Url,
            layout.FooterPage.Footer, layout.FooterPage.Url);

        var built = page.Article != null && SiteImportTemplates.HasGallery(page.Content!)
            ? await gallery.BuildAsync(PrepareContent(page.Content!), page.Url, websiteId, isArticle: false,
                page.SubTitle ?? MenuPageTitle(page, menu), result, checkpoint, token)
            : await BuildPageAsync(page, ownBanner, ownFooter, recipient, result, checkpoint, token);
        if (built == null)
        {
            result.AddError($"選單「{menu.Title}」的來源頁面沒有內容，未覆蓋。");
            return;
        }

        await checkpoint();
        var response = await webMenu.importConten(new MenuSaveContenDto
        {
            Id = checked((int)menu.Id),
            SaveHtml = built.Html,
            SaveCss = ownBanner ? built.Css + "\n" + HideGlobalBanner : built.Css
        });
        if (!response.Success)
        {
            logger.LogWarning("Site crawl content save failed for menu {MenuId}: {Reason}", menu.Id, response.Error);
            result.AddError($"選單「{menu.Title}」的內容儲存失敗，請查看 MVC 日誌。");
            return;
        }

        if (ownFooter) await HideGlobalFooterAsync(menu.Id, websiteId, token);
        if (built.UsedTemplate) result.ArticleTemplatesApplied++;
        result.PagesImported++;
    }

    private async Task<SiteGalleryContent?> BuildPageAsync(SiteLayoutScanner.LayoutPage page, bool ownBanner, bool ownFooter,
        SiteFormRecipient recipient, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var wrapper = HtmlNode.CreateNode("<div></div>");
        if (ownBanner && page.Banner != null)
        {
            // 頁面自己的版頭（例 superiorlink 首頁輪播和內頁不同）是腳本讀出的輪播時，轉成新站輪播，不要變成一疊圖片。
            var banner = HtmlNode.CreateNode("<div></div>");
            banner.AppendChild(page.Banner.CloneNode(true));
            SiteSwiperMarkup.ConvertCarousels(banner);
            foreach (var child in banner.ChildNodes.ToArray()) wrapper.AppendChild(child);
        }
        foreach (var child in PrepareContent(page.Content!).ChildNodes.ToArray()) wrapper.AppendChild(child.CloneNode(true));
        if (ownFooter && page.Footer != null) wrapper.AppendChild(page.Footer.CloneNode(true));

        var forms = SiteFormConverter.Extract(wrapper, recipient);
        var css = await html.CleanAsync(wrapper, page.Url, result, allowIframes: true, checkpoint, token);
        SiteFormConverter.Restore(wrapper, forms);
        result.FormsConverted += forms.Count;
        return HasContent(wrapper)
            ? new SiteGalleryContent(sanitizer.SanitizePublicHtml(wrapper.InnerHtml), sanitizer.SanitizePublicCss(css), false)
            : null;
    }

    private async Task HideGlobalFooterAsync(long menuId, long websiteId, CancellationToken token)
    {
        var entity = await db.WebMenus.FirstOrDefaultAsync(x => x.Id == menuId && x.FK_WebsiteId == websiteId, token);
        if (entity is not { VisibleFooter: true }) return;
        entity.VisibleFooter = false;
        await user.SaveChanges(entity);
    }

    // 舊站沒有 #sub_title 時的備案：左欄給這頁的名稱（例 飯店設施直接連到「泡湯游泳」的文章頁）；左欄沒有這頁時用選單名稱。
    private static string MenuPageTitle(SiteLayoutScanner.LayoutPage page, ImportedSiteMenu menu)
    {
        var key = SiteLayoutScanner.PageKey(page.Url);
        return page.SectionLinks?.FirstOrDefault(link => link.Url != null && SiteLayoutScanner.PageKey(link.Url) == key)?.Title
            ?? menu.SourceTitle.Trim();
    }

    internal static HtmlNode PrepareContent(HtmlNode content)
    {
        var node = content.CloneNode(true);
        if (node.Name == "body")
            foreach (var cover in node.SelectNodes(LoadingCover) ?? Enumerable.Empty<HtmlNode>())
                cover.RemoveClass(HiddenClass);
        foreach (var noise in node.SelectNodes(NoiseSelector()) ?? Enumerable.Empty<HtmlNode>())
            noise.Remove();
        if (node.Name == "body")
            foreach (var chrome in node.SelectNodes(PageChrome) ?? Enumerable.Empty<HtmlNode>())
                chrome.Remove();
        // 舊站載入中狀態：#moduleCont.opacity > .span12 { opacity:0 }，等舊站 JS 載完才拿掉 class；新站沒有那段 JS，內容會一直透明。
        foreach (var loading in node.SelectNodes(LoadingStateSelector) ?? Enumerable.Empty<HtmlNode>())
            loading.RemoveClass(LoadingStateClass);
        SiteSwiperMarkup.ConvertCarousels(node);
        ConvertLegacyGrid(node);
        return node;
    }

    // 新站不搬 Bootstrap 2（使用者決定），舊站多欄的列（例 superiorlink 首頁六個產品分類 .span2）會全部疊成一欄。
    // 有兩欄以上的列補上 Bootstrap 5 格線：md 以上照舊站欄寬並排，md 以下一欄一列（和舊站 bootstrap-responsive 767px 以下相同）。
    // 只加 class 不拿掉舊的，後面切簡述（.row-fluid 當邊界）和舊站自訂樣式都照舊適用。
    private static void ConvertLegacyGrid(HtmlNode node)
    {
        foreach (var row in node.DescendantsAndSelf().Where(x => x.HasClass("row-fluid") || x.HasClass("row")).ToArray())
        {
            var columns = row.ChildNodes
                .Select(child => (child, size: child.NodeType == HtmlNodeType.Element ? SpanSize(child) : null))
                .Where(x => x.size != null)
                .ToArray();
            if (columns.Length < 2) continue;
            row.AddClass("row");
            row.AddClass("g-2");
            foreach (var (column, size) in columns)
            {
                column.AddClass("col-12");
                column.AddClass($"col-md-{size}");
            }
        }
    }

    private static int? SpanSize(HtmlNode node)
    {
        foreach (var name in node.GetClasses())
        {
            var match = System.Text.RegularExpressions.Regex.Match(name, LegacySpan);
            if (match.Success) return int.Parse(match.Groups["size"].Value);
        }
        return null;
    }

    private static string NoiseSelector()
    {
        var classes = NoiseClasses.Select(name => $"contains(concat(' ',normalize-space(@class),' '),' {name} ')");
        var ids = NoiseIds.Select(id => $"@id='{id}'");
        return $".//comment()|.//*[{string.Join(" or ", classes.Concat(ids))}]";
    }

    internal static bool HasContent(HtmlNode node) =>
        !string.IsNullOrWhiteSpace(HtmlEntity.DeEntitize(node.InnerText)) || node.SelectSingleNode(".//img|.//iframe|.//form") != null;

    // 舊站表單的收件信箱存在舊站後台、HTML 抓不到，使用者決定先一律用佔位信箱，上線前再到編輯器改成正式信箱。
    private const string PlaceholderFormRecipient = "temple@testmail.com";

    private async Task<SiteFormRecipient> FormRecipientAsync(long websiteId, CancellationToken token)
    {
        var site = await db.Websites.Where(x => x.Id == websiteId)
            .Select(x => new { x.Contact, x.Title })
            .FirstOrDefaultAsync(token);
        var title = string.IsNullOrWhiteSpace(site?.Contact) ? site?.Title ?? "" : site.Contact;
        return new SiteFormRecipient(PlaceholderFormRecipient, title);
    }
}
