using HtmlAgilityPack;

namespace EtheriT.Coker.Application.SiteCrawl;

// 新站前台內建的輪播結構：coker.page.init.js 看到內文有 .one_swiper／.one_swiper_thumbs 就呼叫 SwiperInit 啟動，
// 不需要另外設定元件樣板（2026-11-16 使用者決定：舊站輪播一律直接轉成這個結構）。
internal static class SiteSwiperMarkup
{
    private const string Carousel =
        ".//*[contains(concat(' ',normalize-space(@class),' '),' carousel ')]" +
        "[.//*[contains(concat(' ',normalize-space(@class),' '),' carousel-inner ')]]";
    private const string CarouselInner = ".//*[contains(concat(' ',normalize-space(@class),' '),' carousel-inner ')]";
    // Bootstrap 2／3 用 .item，4／5 用 .carousel-item。
    private const string CarouselItems =
        "./*[contains(concat(' ',normalize-space(@class),' '),' item ') or contains(concat(' ',normalize-space(@class),' '),' carousel-item ')]";
    private const string SlideClass = "swiper-slide d-flex justify-content-center align-items-center";
    // 前台 Swiper.js 看到這個屬性就自動播放（舊站輪播原本會自己切換，使用者 2026-11-16 要求）。
    internal const string AutoplayAttribute = "data-autoplay";
    private const string SwiperRoot =
        "ancestor::*[contains(concat(' ',normalize-space(@class),' '),' one_swiper ') or contains(concat(' ',normalize-space(@class),' '),' one_swiper_thumbs ')][1]";

    // 使用者的樣板也一樣自動播放：放了相簿圖片的 swiper-wrapper，往上找到 .one_swiper／.one_swiper_thumbs 加屬性。
    internal static void EnableAutoplay(HtmlNode wrapper) =>
        wrapper.SelectSingleNode(SwiperRoot)?.SetAttributeValue(AutoplayAttribute, "true");
    private const string ImageClass = "img-fluid";

    private const string Buttons =
        """<div class="swiper_button_prev start-0 position-absolute top-50 translate-middle-y">""" +
        """<button type="button" class="align-items-center bg-dark bg-opacity-25 border-0 d-flex justify-content-center rounded-circle swiper_btn_circle_small text-opacity-75 text-white"><span class="material-symbols-outlined">navigate_before</span></button></div>""" +
        """<div class="swiper_button_next end-0 position-absolute top-50 translate-middle-y">""" +
        """<button type="button" class="align-items-center bg-dark bg-opacity-25 border-0 d-flex justify-content-center rounded-circle swiper_btn_circle_small text-opacity-75 text-white"><span class="material-symbols-outlined">navigate_next</span></button></div>""";

    // 沒有勾選「匯入用文章樣板」元件時的相簿版面：md 以上左相簿右文字，md 以下上下排；
    // 文字位置和使用者的樣板一樣用 .article_title／.introduction_title／.introduction_area，由 SiteImportTemplates.FillSlots 填入。
    // 縮圖由前台 SwiperInit 依主圖自動產生，所以 .six_thumbs 的 wrapper 留空。
    internal const string GalleryTemplateHtml =
        """<div class="container crawl-gallery"><div class="row g-4 mb-4">""" +
        """<div class="col-12 col-md-6"><div class="one_swiper_thumbs" data-autoplay="true"><div class="swiper">""" +
        """<div class="swiper-wrapper"><div class="swiper-slide"><img class="w-100" src="" alt=""></div></div>""" +
        Buttons +
        """</div><div class="six_thumbs swiper-thumbs"><div class="swiper-wrapper"></div></div></div></div>""" +
        """<div class="col-12 col-md-6"><h2 class="article_title fw-bold mb-3"></h2>""" +
        """<h3 class="introduction_title fw-bold mb-3"></h3><div class="introduction_area"></div></div>""" +
        """</div><div data-import-role="article-content"></div></div>""";

    internal const string GalleryTemplateCss =
        ".crawl-gallery .one_swiper_thumbs > .swiper img{display:block;height:auto}\n" +
        ".crawl-gallery .six_thumbs{margin-top:.5rem}\n" +
        ".crawl-gallery .six_thumbs .swiper-slide{cursor:pointer;opacity:.6}\n" +
        ".crawl-gallery .six_thumbs .swiper-slide-thumb-active{opacity:1}\n" +
        ".crawl-gallery .six_thumbs img{display:block;width:100%;aspect-ratio:1/1;object-fit:cover}";

    // 舊站內文的 Bootstrap 輪播（例 chiyangcoffee 首頁 #myCarousel）原地換成 .one_swiper。
    // 每個 .item 的內容整段搬進 slide（含連結、說明文字），舊的左右箭頭與指示點拿掉，改用新站的按鈕。
    internal static void ConvertCarousels(HtmlNode content)
    {
        foreach (var carousel in content.SelectNodes(Carousel)?.ToArray() ?? [])
        {
            if (carousel.ParentNode == null) continue; // 已隨外層輪播一起被換掉
            var items = carousel.SelectSingleNode(CarouselInner)?.SelectNodes(CarouselItems)?.ToArray() ?? [];
            if (items.Length == 0) continue;
            var wrapper = HtmlNode.CreateNode("""<div class="swiper-wrapper"></div>""");
            foreach (var item in items) wrapper.AppendChild(CreateSlide(item));
            var swiper = HtmlNode.CreateNode("""<div class="swiper"></div>""");
            swiper.AppendChild(wrapper);
            if (items.Length > 1)
                foreach (var button in HtmlNode.CreateNode($"<div>{Buttons}</div>").ChildNodes.ToArray())
                    swiper.AppendChild(button);
            var root = HtmlNode.CreateNode($"""<div class="one_swiper" {AutoplayAttribute}="true"></div>""");
            root.AppendChild(swiper);
            carousel.ParentNode.ReplaceChild(root, carousel);
        }
    }

    private static HtmlNode CreateSlide(HtmlNode item)
    {
        var slide = HtmlNode.CreateNode($"""<div class="{SlideClass}"></div>""");
        foreach (var child in item.ChildNodes.ToArray()) slide.AppendChild(child.CloneNode(true));
        foreach (var image in slide.Descendants("img")) image.AddClass(ImageClass);
        return slide;
    }
}
