using HtmlAgilityPack;

namespace EtheriT.Coker.Application.SiteCrawl;

internal sealed record SiteGallerySlide(string Src, string Alt);
internal sealed record SiteTemplateSlots(bool Title, bool IntroductionTitle, bool Introduction);
internal sealed record SiteArticleIntro(string? Title, HtmlNode? Introduction);

// 匯入時套用元件樣板的 HTML 處理（2026-11-13 對 ayawan.com.tw 實測）：
// 1. 列表頁：舊站列表上方常有分類共用圖（例 房型價格表），在 #moduleCont 之外，改用目錄元件時要保留在目錄前面。
// 2. 相簿文章：內文有 <a href="#modalOfSwiper"> 主圖＋.preview_img 縮圖，點了由舊站 JS 開輪播視窗；
//    新站改用勾選「匯入用文章樣板」的元件，把這些圖片放進樣板的 swiper，其餘內文放進 data-import-role="article-content"。
internal static class SiteImportTemplates
{
    internal const string ArticleTemplateCode = "article-import-template";
    private const string ListModule = ".//*[@id='moduleCont']";
    private const string ListLinks =
        ".//a[@href][contains(concat(' ',normalize-space(@class),' '),' newsItem ') or contains(concat(' ',normalize-space(@class),' '),' prod_link ')]";
    private const string GalleryTrigger = ".//a[@href='#modalOfSwiper']";
    private const string GalleryThumbs = ".//*[contains(concat(' ',normalize-space(@class),' '),' preview_img ')]";
    private const string SwiperWrapper = "//*[contains(concat(' ',normalize-space(@class),' '),' swiper-wrapper ')]";
    private const string SwiperSlide = "./*[contains(concat(' ',normalize-space(@class),' '),' swiper-slide ')]";
    private const string ArticleContentMarker = "//*[@data-import-role='article-content']";
    private const string SourceTitle = ".//*[contains(concat(' ',normalize-space(@class),' '),' menu_title ')]";
    // 樣板裡可被覆蓋的文字位置（使用者在元件上加的 class）：
    // .article_title 填舊站 #sub_title；.introduction_title 填舊站 .menu_title；
    // .introduction_area 填舊站相簿旁的簡述。舊站沒有對應內容的位置整個移除。
    private const string SlotTitle = "//*[contains(concat(' ',normalize-space(@class),' '),' article_title ')]";
    private const string SlotIntroTitle = "//*[contains(concat(' ',normalize-space(@class),' '),' introduction_title ')]";
    private const string SlotIntroArea = "//*[contains(concat(' ',normalize-space(@class),' '),' introduction_area ')]";
    // 簡述到這裡為止：舊站版面列（.row／.row-fluid）、JS 載入區塊（.contItem）、上下則（#list_ctrl）。
    // 一般的 div（例 <div style="text-align: justify;">）是簡述本身，不能當結尾（2026-11-16 飯店設施實測）。
    // 簡述的結束點：下一個版面列、上下則、contItem；或舊站用來讓內容換到相簿下方的 clear
    // （例 superiorlink 商品頁 <br clear="all"> 之後才是特性、技術規範，2026-11-17 實測）。
    private const string IntroBoundary =
        "self::*[contains(concat(' ',normalize-space(@class),' '),' row ') or contains(concat(' ',normalize-space(@class),' '),' row-fluid ')]" +
        " | self::br[@clear] | self::*[contains(translate(@style,'CLEAR','clear'),'clear')]" +
        " | descendant-or-self::*[@id='list_ctrl' or contains(concat(' ',normalize-space(@class),' '),' contItem ')]";

    // 元件（Html_Contents.Html）有的被編碼兩次（例 70803 存成 &amp;lt;div），只解一次會變成純文字、找不到任何元素。
    // 比照後台編輯器 co.Data.HtmlDecode（Default.js ReplaceAndSinge）：先把 &amp; 反覆換成 &，再解碼一次。
    internal static string DecodeComponentHtml(string? html)
    {
        var value = html ?? "";
        while (value.Contains("&amp;", StringComparison.Ordinal))
            value = value.Replace("&amp;", "&", StringComparison.Ordinal);
        return System.Net.WebUtility.HtmlDecode(value);
    }

    // 回傳列表區塊以外的內容（已套用 PrepareContent）；找不到列表區塊時回傳 null，避免整頁內容和目錄重複。
    internal static HtmlNode? ExtractListIntro(HtmlNode content)
    {
        var node = SiteContentImporter.PrepareContent(content);
        var module = node.SelectSingleNode(ListModule) ?? CommonAncestor(node.SelectNodes(ListLinks));
        if (module == null || module == node) return null;
        module.Remove();
        return SiteContentImporter.HasContent(node) ? node : null;
    }

    private static HtmlNode? CommonAncestor(HtmlNodeCollection? links)
    {
        if (links == null || links.Count == 0) return null;
        var common = links[0].AncestorsAndSelf().ToList();
        foreach (var link in links.Skip(1))
        {
            var ancestors = link.AncestorsAndSelf().ToHashSet();
            common = common.Where(ancestors.Contains).ToList();
        }
        return common.FirstOrDefault();
    }

    // 前台 .menu_directory 元件（後台編輯器「目錄」元件的標準結構），由 data-dirid 指向選單類型目錄。
    // .title 先填目錄名稱：前台主選單只給「有文字或圖片」的頁面連結（CustomDtoMapper.HasContent），空標題會讓這頁點不進去；
    // 前台載入時 JS 會再用目錄名稱覆寫一次。
    internal static string BuildMenuDirectory(long directoryId, string title)
    {
        var text = HtmlEntity.Entitize(title, true, true);
        return $"""<div class="container"><div class="menu_directory bg-white" data-dirid="{directoryId}" data-diridname="{text}">""" +
        $"""<div class="title custom_h5 fw-bold py-3 px-3">{text}</div><div class="accordion accordion-flush"></div>""" +
        """<div data-role="menu-accordion-template" class="d-none"><div class="accordion-item border-0 border-bottom">""" +
        """<div class="accordion-header"><button type="button" data-bs-toggle="collapse" data-bs-target="" aria-expanded="false" aria-controls="" class="accordion-button collapsed custom_h5 sectitle"></button></div>""" +
        """<div aria-labelledby="" class="accordion-collapse collapse"><div class="accordion-body p-0"></div></div></div></div></div></div>""";
    }

    internal static bool HasGallery(HtmlNode content) => content.SelectSingleNode(GalleryTrigger) != null;

    // 取出相簿圖片並從內文移除（縮圖已含主圖；沒有縮圖時才用主圖）。
    // 圖片列表頁（例 chiyangcoffee 季洋隨行吧：飲品、輕食兩張卡片都開同一個輪播）會有多個觸發連結，每個的主圖都要收。
    internal static List<HtmlNode> TakeGalleryImages(HtmlNode content)
    {
        var images = new List<HtmlNode>();
        foreach (var thumbs in content.SelectNodes(GalleryThumbs)?.ToArray() ?? [])
        {
            images.AddRange(thumbs.Descendants("img").Select(image => image.CloneNode(true)));
            thumbs.Remove();
        }
        var useTriggerImages = images.Count == 0;
        foreach (var trigger in content.SelectNodes(GalleryTrigger)?.ToArray() ?? [])
        {
            if (useTriggerImages) images.AddRange(trigger.Descendants("img").Select(image => image.CloneNode(true)));
            trigger.Remove();
        }
        return images;
    }

    internal static bool HasSlidePrototype(string templateHtml)
    {
        var document = new HtmlDocument();
        document.LoadHtml(templateHtml);
        return document.DocumentNode.SelectNodes(SwiperWrapper)?.Any(wrapper => wrapper.SelectSingleNode(SwiperSlide) != null) == true;
    }

    internal static SiteTemplateSlots ReadSlots(string templateHtml)
    {
        var document = new HtmlDocument();
        document.LoadHtml(templateHtml);
        var root = document.DocumentNode;
        return new SiteTemplateSlots(root.SelectSingleNode(SlotTitle) != null, root.SelectSingleNode(SlotIntroTitle) != null,
            root.SelectSingleNode(SlotIntroArea) != null);
    }

    internal static string? ReadHeading(HtmlNode content)
    {
        var title = content.SelectSingleNode(SourceTitle);
        var text = title == null ? "" : HtmlEntity.DeEntitize(title.InnerText).Trim();
        return text.Length == 0 ? null : text;
    }

    // 舊站相簿文章的版面（2026-11-16 ayawan 實測）：左邊相簿，右邊是 .menu_title 標題，
    // 標題後面、到下一個版面列（.row-fluid 的產品說明、上下則）之前的內容就是相簿旁的簡述。
    // 依樣板有沒有對應位置決定要不要拿走；拿走的節點會從內文移除，回傳簡述的容器（沒有文字時為 null）。
    internal static SiteArticleIntro TakeIntroduction(HtmlNode content, bool takeTitle, bool takeText)
    {
        var title = content.SelectSingleNode(SourceTitle);
        if (title == null) return new SiteArticleIntro(null, null);
        var block = title;
        while (block.ParentNode != null && block.ParentNode != content && block.ParentNode.ChildNodes.Count(IsMeaningful) == 1)
            block = block.ParentNode;

        HtmlNode? intro = null;
        if (takeText)
        {
            SplitAtNestedClear(block);
            var holder = HtmlNode.CreateNode("<div></div>");
            for (var node = block.NextSibling; node != null && !IsBlockBoundary(node); )
            {
                var next = node.NextSibling;
                holder.AppendChild(node.CloneNode(true));
                node.Remove();
                node = next;
            }
            TrimBreaks(holder);
            intro = SiteContentImporter.HasContent(holder) ? holder : null;
        }
        var text = HtmlEntity.DeEntitize(title.InnerText).Trim();
        if (takeTitle) block.Remove();
        return new SiteArticleIntro(text.Length == 0 ? null : text, intro);
    }

    private const string NestedClear = ".//br[@clear] | .//*[contains(translate(@style,'CLEAR','clear'),'clear')]";

    // 舊站常把簡述和詳細內容寫在同一個 div 裡，中間用 <br clear> 換到相簿下方
    // （例 superiorlink 商品 402：<div style="text-align: justify;">簡述<br clear="all"><h3>特性</h3>…）。
    // 在 clear 處把外層拆成前後兩段、clear 提到兩段中間，簡述就只拿到前段。
    private static void SplitAtNestedClear(HtmlNode block)
    {
        for (var node = block.NextSibling; node != null && !IsBlockBoundary(node); node = node.NextSibling)
        {
            if (node.NodeType != HtmlNodeType.Element || node.SelectSingleNode(NestedClear) is not { } marker) continue;
            var tail = SplitOff(node, marker);
            marker.Remove();
            node.ParentNode.InsertAfter(marker, node);
            if (tail.HasChildNodes) node.ParentNode.InsertAfter(tail, marker);
            return;
        }
    }

    // 把 node 從 marker（含）以後的內容搬到 node 的淺層複本（保留外層屬性），回傳該複本。
    private static HtmlNode SplitOff(HtmlNode node, HtmlNode marker)
    {
        var tail = node.CloneNode(false);
        var child = marker;
        while (child.ParentNode != node) child = child.ParentNode;
        var start = child == marker ? child : child.NextSibling;
        if (child != marker) tail.AppendChild(SplitOff(child, marker));
        for (var current = start; current != null;)
        {
            var next = current.NextSibling;
            current.Remove();
            tail.AppendChild(current);
            current = next;
        }
        return tail;
    }

    private static bool IsMeaningful(HtmlNode node) =>
        node.NodeType == HtmlNodeType.Element
        || (node.NodeType == HtmlNodeType.Text && !string.IsNullOrWhiteSpace(HtmlEntity.DeEntitize(node.InnerText)));

    private static bool IsBlockBoundary(HtmlNode node) =>
        node.NodeType == HtmlNodeType.Element && node.SelectSingleNode(IntroBoundary) != null;

    private static void TrimBreaks(HtmlNode holder)
    {
        while (holder.FirstChild is { } first && IsBreakOrBlank(first)) first.Remove();
        while (holder.LastChild is { } last && IsBreakOrBlank(last)) last.Remove();
    }

    private static bool IsBreakOrBlank(HtmlNode node) =>
        node.Name == "br" || (node.NodeType == HtmlNodeType.Text && string.IsNullOrWhiteSpace(HtmlEntity.DeEntitize(node.InnerText)));

    // 移除樣板位置時一併移除緊接在後的 <br>，否則會留下空行。
    private static void RemoveWithBreaks(HtmlNode node)
    {
        while (node.NextSibling is { } next && IsBreakOrBlank(next)) next.Remove();
        node.Remove();
    }

    // 每個 swiper-wrapper 以第一張 slide 當範本，依圖片數量複製；introHtml、contentHtml 須已淨化。
    internal static string BuildGalleryArticle(string templateHtml, IReadOnlyList<SiteGallerySlide> slides,
        string? title, string? introTitle, string? introHtml, string contentHtml)
    {
        var document = new HtmlDocument();
        document.LoadHtml(templateHtml);
        FillSlots(document.DocumentNode, title, introTitle, introHtml);
        foreach (var wrapper in document.DocumentNode.SelectNodes(SwiperWrapper) ?? Enumerable.Empty<HtmlNode>())
        {
            var existing = wrapper.SelectNodes(SwiperSlide)?.ToArray() ?? [];
            if (existing.Length == 0) continue;
            var prototype = existing[0];
            foreach (var slide in existing) slide.Remove();
            foreach (var item in slides)
                wrapper.AppendChild(CreateSlide(prototype, item));
            SiteSwiperMarkup.EnableAutoplay(wrapper);
        }
        var holder = document.DocumentNode.SelectSingleNode(ArticleContentMarker);
        if (holder == null)
        {
            holder = HtmlNode.CreateNode("<div></div>");
            ContentRoot(document).AppendChild(holder);
        }
        holder.InnerHtml = contentHtml;
        return BodyHtml(document);
    }

    // 元件樣板的 HTML 可能整段包在 <body> 裡；發布時只取 <body> 內容，接在 <body> 外的東西會被丟掉，
    // 所以一律只回傳 <body> 內層，再由呼叫端在前後接內容（2026-11-13 首頁價格圖消失的原因）。
    internal static string BodyHtml(HtmlDocument document) =>
        document.DocumentNode.SelectSingleNode("//body")?.InnerHtml ?? document.DocumentNode.OuterHtml;

    // 樣板範例文字一律換掉或移除，舊站沒有簡述時不留範例文字。
    private static void FillSlots(HtmlNode root, string? title, string? introTitle, string? introHtml)
    {
        if (title != null)
            foreach (var node in root.SelectNodes(SlotTitle)?.ToArray() ?? [])
                node.InnerHtml = System.Net.WebUtility.HtmlEncode(title);
        foreach (var node in root.SelectNodes(SlotIntroTitle)?.ToArray() ?? [])
        {
            if (introTitle == null) RemoveWithBreaks(node);
            else node.InnerHtml = System.Net.WebUtility.HtmlEncode(introTitle);
        }
        foreach (var node in root.SelectNodes(SlotIntroArea)?.ToArray() ?? [])
        {
            if (introHtml == null) RemoveWithBreaks(node);
            else node.InnerHtml = introHtml;
        }
    }

    private static HtmlNode ContentRoot(HtmlDocument document) =>
        document.DocumentNode.SelectSingleNode("//body") ?? document.DocumentNode;

    private static HtmlNode CreateSlide(HtmlNode prototype, SiteGallerySlide item)
    {
        var slide = prototype.CloneNode(true);
        if (slide.Attributes["data-coker-slide-id"] != null)
            slide.SetAttributeValue("data-coker-slide-id", Guid.NewGuid().ToString());
        if (slide.SelectSingleNode(".//img") is { } image)
        {
            image.SetAttributeValue("src", item.Src);
            image.SetAttributeValue("alt", item.Alt);
            image.Attributes.Remove("srcset");
            image.Attributes.Remove("data-src");
        }
        if (slide.SelectSingleNode(".//a[@title]") is { } link)
            link.SetAttributeValue("title", item.Alt);
        return slide;
    }
}
