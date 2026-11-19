using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Processor;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.Application.SiteCrawl;

internal sealed record SiteGalleryContent(string Html, string Css, bool UsedTemplate);

// 相簿內文（#modalOfSwiper）套「匯入用文章樣板」：文章匯入與「直接連到文章詳細頁的選單」（例 觀猴套房）共用。
// 一次匯入工作內只查一次樣板；沒有樣板元件時用內建版面。
public sealed class SiteGalleryBuilder(SiteCrawlHtml html, IHtmlSanitizer sanitizer, CokerDbContext db)
{
    private sealed record GalleryTemplate(string Html, string Css, SiteTemplateSlots Slots);

    private GalleryTemplate? template;

    internal Task<Html_Content?> FindTemplateAsync(long websiteId, string purposeCode, CancellationToken token) =>
        db.Html_Contents
            .Where(x => !x.IsDeleted && x.Disp_opt
                && (x.Type != (int)ObjectTypeEnum.自訂 || x.FK_WebsiteId == websiteId)
                && x.HtmlContentPurposes.Any(p => !p.IsDeleted && p.ComponentPurpose.Visible && p.ComponentPurpose.Code == purposeCode))
            .OrderByDescending(x => x.FK_WebsiteId == websiteId)
            .ThenBy(x => x.Ser_no)
            .ThenBy(x => x.Id)
            .FirstOrDefaultAsync(token);

    // content 須已經過 PrepareContent，會被修改；回傳 null 代表沒有可存的內容。
    // isArticle：文章標題已存成文章標題欄位，內文不再保留舊站標題。
    // 樣板文字位置（使用者 2026-11-16 指定）：.article_title 填舊站 sub_title（例 泡湯游泳、花鹿三追一），
    // .introduction_title 填舊站 .menu_title（例 露天溫泉池、兩大床有泡湯池客廳VIP房），兩者相同時照舊站一樣都顯示。
    // subTitle 沒有時才用 .menu_title 當 .article_title。
    internal async Task<SiteGalleryContent?> BuildAsync(HtmlNode content, Uri pageUrl, long websiteId, bool isArticle,
        string? subTitle, SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var galleryTemplate = SiteImportTemplates.HasGallery(content) ? await GetTemplateAsync(websiteId, result, token) : null;
        var gallery = galleryTemplate == null ? [] : SiteImportTemplates.TakeGalleryImages(content);
        var slides = await ImportSlidesAsync(gallery, pageUrl, result, checkpoint, token);
        var applied = slides.Count > 0 ? galleryTemplate : null;
        var heading = SiteImportTemplates.ReadHeading(content);
        var title = string.IsNullOrWhiteSpace(subTitle) ? heading : subTitle.Trim();
        var introTitle = string.IsNullOrWhiteSpace(subTitle) ? null : heading;
        var headingSlot = introTitle == null ? applied?.Slots.Title : applied?.Slots.IntroductionTitle;
        var intro = SiteImportTemplates.TakeIntroduction(content,
            takeTitle: isArticle || headingSlot == true,
            takeText: applied?.Slots.Introduction == true);

        var wrapper = HtmlNode.CreateNode("<div></div>");
        foreach (var child in content.ChildNodes.ToArray()) wrapper.AppendChild(child.CloneNode(true));
        var css = await html.CleanAsync(wrapper, pageUrl, result, allowIframes: true, checkpoint, token);
        if (applied == null && !SiteContentImporter.HasContent(wrapper)) return null;

        var contentHtml = sanitizer.SanitizePublicHtml(wrapper.InnerHtml);
        var contentCss = sanitizer.SanitizePublicCss(css);
        if (applied == null) return new SiteGalleryContent(contentHtml, contentCss, false);

        var (introHtml, introCss) = await CleanIntroductionAsync(intro.Introduction, pageUrl, result, checkpoint, token);
        return new SiteGalleryContent(
            SiteImportTemplates.BuildGalleryArticle(applied.Html, slides, title, introTitle, introHtml, contentHtml),
            applied.Css + "\n" + introCss + contentCss, true);
    }

    private async Task<(string? Html, string Css)> CleanIntroductionAsync(HtmlNode? intro, Uri pageUrl,
        SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        if (intro == null) return (null, "");
        var css = await html.CleanAsync(intro, pageUrl, result, allowIframes: true, checkpoint, token);
        if (!SiteContentImporter.HasContent(intro)) return (null, "");
        var cleanCss = sanitizer.SanitizePublicCss(css);
        return (sanitizer.SanitizePublicHtml(intro.InnerHtml), cleanCss.Length == 0 ? "" : cleanCss + "\n");
    }

    // 沒有勾選「匯入用文章樣板」的元件時改用內建版面（SiteSwiperMarkup），正式站不用先設樣板也能用（使用者 2026-11-16 決定）。
    private async Task<GalleryTemplate> GetTemplateAsync(long websiteId, SiteLayoutResult result, CancellationToken token)
    {
        if (template != null) return template;
        var found = await FindTemplateAsync(websiteId, SiteImportTemplates.ArticleTemplateCode, token);
        var decoded = SiteImportTemplates.DecodeComponentHtml(found?.Html);
        if (found != null && SiteImportTemplates.HasSlidePrototype(decoded))
            return template = new GalleryTemplate(decoded, found.Css ?? "", SiteImportTemplates.ReadSlots(decoded));
        if (found != null)
            result.AddError("「匯入用文章樣板」元件裡找不到 swiper-wrapper／swiper-slide，相簿改用內建版面。");
        var builtIn = SiteSwiperMarkup.GalleryTemplateHtml;
        return template = new GalleryTemplate(builtIn, SiteSwiperMarkup.GalleryTemplateCss, SiteImportTemplates.ReadSlots(builtIn));
    }

    private async Task<List<SiteGallerySlide>> ImportSlidesAsync(IEnumerable<HtmlNode> images, Uri pageUrl,
        SiteLayoutResult result, Func<Task> checkpoint, CancellationToken token)
    {
        var slides = new List<SiteGallerySlide>();
        foreach (var image in images)
        {
            var path = await html.ImportImageAsync(SiteCrawlHtml.ImageSource(image), pageUrl, result, checkpoint, token);
            if (path == null || slides.Any(slide => slide.Src == path)) continue;
            var alt = HtmlEntity.DeEntitize(image.GetAttributeValue("alt", "")).Replace('"', '\'').Trim();
            slides.Add(new SiteGallerySlide(path, alt));
        }
        return slides;
    }
}
