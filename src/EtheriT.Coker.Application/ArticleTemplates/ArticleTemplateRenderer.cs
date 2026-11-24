using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Application.Shared.Search;
using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;

namespace EtheriT.Coker.Application.ArticleTemplates;

// 樣板＋欄位資料 → 文章 HTML（未編碼的原始 HTML，存檔前由呼叫端照文章既有流程 HtmlEncode）。
// 所有欄位值都當純文字處理，一律編碼後才放進 HTML。
public static class ArticleTemplateRenderer
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex DomainPattern = new(@"^[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+(/\S*)?$", RegexOptions.Compiled);

    public static string Render(
        ArticleTemplate template,
        IReadOnlyDictionary<string, List<string>> values,
        string orgName,
        IReadOnlyList<string>? tags = null)
    {
        var document = ArticleTemplateCatalog.ParseHtml(template.Html);
        var pageTitle = ArticleFieldValues.First(values, template.PageTitleKey);
        var filledKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in ArticleTemplateCatalog.KeyedNodes(document.DocumentNode).ToList())
        {
            var key = ArticleTemplateCatalog.EditKey(node);
            var type = ArticleTemplateCatalog.EditType(node);
            var all = ArticleFieldValues.All(values, key);
            var first = all.FirstOrDefault() ?? "";
            if (all.Count > 0 && type != ArticleFieldTypes.Section)
                filledKeys.Add(key);

            switch (type)
            {
                case ArticleFieldTypes.PageTitle:
                case ArticleFieldTypes.String:
                    SetText(node, first.Length == 0 ? "" : Format(node, first, 1));
                    break;
                case ArticleFieldTypes.Textarea:
                    SetMultiline(node, first);
                    break;
                case ArticleFieldTypes.Link:
                    FillLink(node, first);
                    break;
                case ArticleFieldTypes.Image:
                    if (!IsImageUrl(first)) filledKeys.Remove(key);
                    FillImage(node, first, $"{pageTitle} {LabelText(node)}".Trim());
                    break;
                case ArticleFieldTypes.List:
                    FillItems(node, all, (item, value, index) => FillListItem(item, value, index, orgName));
                    break;
                case ArticleFieldTypes.Gallery:
                    var max = node.GetAttributeValue("data-edit-maxcount", 0);
                    var images = all.Where(IsImageUrl).Take(max > 0 ? max : int.MaxValue).ToList();
                    if (images.Count == 0) filledKeys.Remove(key);
                    FillItems(node, images, (item, value, index) => FillGalleryItem(item, value, $"{pageTitle} 圖片 {index}".Trim()));
                    // 隱藏的範本項目留著，相簿燈箱會把它當成一張圖（出現預設佔位圖），所以填完就移除。
                    ArticleTemplateCatalog.ItemTemplate(node)?.Remove();
                    break;
            }
        }

        FillMaps(document, values, pageTitle);
        FillTags(document, tags ?? [], orgName);
        RemoveEmptyBlocks(document, filledKeys);
        return document.DocumentNode.OuterHtml;
    }

    // Tags：系統帶入文章實際掛的標籤；沒有標籤就整塊拿掉。
    private static void FillTags(HtmlDocument document, IReadOnlyList<string> tags, string orgName)
    {
        var nodes = document.DocumentNode.Descendants()
            .Where(n => ArticleTemplateCatalog.EditType(n) == ArticleFieldTypes.Tags)
            .ToList();
        foreach (var node in nodes)
        {
            if (tags.Count == 0)
            {
                node.Remove();
                continue;
            }
            FillItems(node, tags, (item, value, index) => FillListItem(item, value, index, orgName));
        }
    }

    private static void FillMaps(HtmlDocument document, IReadOnlyDictionary<string, List<string>> values, string pageTitle)
    {
        var maps = document.DocumentNode.Descendants()
            .Where(n => ArticleTemplateCatalog.EditType(n) == ArticleFieldTypes.Map)
            .ToList();
        foreach (var map in maps)
        {
            var address = ArticleFieldValues.First(values, map.GetAttributeValue("data-edit-ref", "").Trim());
            if (address.Length == 0)
            {
                map.Remove();
                continue;
            }
            map.SetAttributeValue("src",
                "https://maps.google.com/maps?q=" + Uri.EscapeDataString(address) + "&amp;z=15&amp;output=embed");
            if (pageTitle.Length > 0)
                SetAttr(map, "title", $"{pageTitle}位置地圖");
        }
    }

    // data-edit-empty="hide"：裡面一個有值的欄位都沒有，就整個拿掉。由內往外處理。
    private static void RemoveEmptyBlocks(HtmlDocument document, HashSet<string> filledKeys)
    {
        var blocks = document.DocumentNode.Descendants()
            .Where(n => string.Equals(n.GetAttributeValue("data-edit-empty", ""), "hide", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(n => n.Ancestors().Count())
            .ToList();
        foreach (var block in blocks)
        {
            var hasValue = block.DescendantsAndSelf().Any(n =>
                ArticleTemplateCatalog.EditType(n) == ArticleFieldTypes.Map
                || ArticleTemplateCatalog.EditType(n) == ArticleFieldTypes.Tags
                || filledKeys.Contains(ArticleTemplateCatalog.EditKey(n)));
            if (!hasValue)
                block.Remove();
        }
    }

    private static void FillItems(HtmlNode field, IReadOnlyList<string> values, Action<HtmlNode, string, int> fill)
    {
        var template = ArticleTemplateCatalog.ItemTemplate(field);
        if (template?.ParentNode == null) return;
        for (var i = 0; i < values.Count; i++)
        {
            var item = template.CloneNode(true);
            item.Attributes.Remove("id");
            RemoveClass(item, "d-none");
            fill(item, values[i], i + 1);
            template.ParentNode.AppendChild(item);
        }
    }

    // List 子項目：文字放進 String／Link；Link 一律連到前台站內搜尋（找文章）。
    private static void FillListItem(HtmlNode item, string value, int index, string orgName)
    {
        foreach (var node in item.DescendantsAndSelf().ToList())
        {
            var type = ArticleTemplateCatalog.EditType(node);
            if (type == ArticleFieldTypes.String)
            {
                SetText(node, Format(node, value, index));
            }
            else if (type == ArticleFieldTypes.Link)
            {
                if (orgName.Length > 0)
                {
                    var href = $"/{orgName}/Search/Get/{SearchTargetIds.Article}/{SearchKeywordCodec.Encode(value)}";
                    SetAttr(node, "href", href);
                    SetAttr(node, "data-link-value", href);
                    SetAttr(node, "data-link-type", "link");
                }
                SetAttr(node, "data-text", value);
                SetText(node, Format(node, value, index));
            }
        }
    }

    private static void FillGalleryItem(HtmlNode item, string url, string alt)
    {
        SetAttr(item, "data-full-src", url);
        var image = item.DescendantsAndSelf().FirstOrDefault(n => n.Name == "img");
        if (image == null) return;
        SetAttr(image, "src", url);
        SetAttr(image, "data-src", url);
        SetAttr(image, "alt", alt);
    }

    private static void FillImage(HtmlNode node, string url, string alt)
    {
        if (!IsImageUrl(url))
        {
            node.Attributes.Remove("src");
            return;
        }
        SetAttr(node, "src", url);
        SetAttr(node, "alt", alt);
    }

    private static void FillLink(HtmlNode node, string value)
    {
        var (href, linkType, linkValue) = ResolveLink(value, node.GetAttributeValue("data-link-type", ""));
        if (href == null)
        {
            node.Attributes.Remove("href");
            node.Attributes.Remove("target");
            node.Attributes.Remove("data-link-value");
        }
        else
        {
            SetAttr(node, "href", href);
            SetAttr(node, "data-link-value", linkValue);
            SetAttr(node, "data-link-type", linkType);
        }
        SetAttr(node, "data-text", value);
        // 版型裡自己放了內容（圖示等）就不覆蓋，只設連結。
        if (!node.ChildNodes.Any(c => c.NodeType == HtmlNodeType.Element))
            SetText(node, value.Length == 0 ? "" : Format(node, value, 1));
    }

    // 回傳 (href, data-link-type, data-link-value)；無法判斷成連結時 href 為 null，只顯示文字。
    internal static (string? Href, string LinkType, string LinkValue) ResolveLink(string value, string preferredType)
    {
        var text = value.Trim();
        if (text.Length == 0) return (null, "link", "");

        if (string.Equals(preferredType, "phone", StringComparison.OrdinalIgnoreCase))
        {
            var phone = ToInternationalPhone(text);
            return phone == null ? (null, "phone", "") : ("tel:" + phone, "phone", phone);
        }
        if (EmailPattern.IsMatch(text))
            return ("mailto:" + text, "email", text);
        if (Uri.TryCreate(text, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            return (absolute.AbsoluteUri, "link", absolute.AbsoluteUri);
        if (DomainPattern.IsMatch(text) && Uri.TryCreate("https://" + text, UriKind.Absolute, out var withScheme))
            return (withScheme.AbsoluteUri, "link", withScheme.AbsoluteUri);
        return (null, "link", "");
    }

    // 「02-82278589#867」→「+886282278589」：分機（#、轉、ext）之後捨去，0 開頭視為台灣號碼。
    internal static string? ToInternationalPhone(string text)
    {
        var main = Regex.Split(text, @"#|轉|ext\.?|分機", RegexOptions.IgnoreCase)[0];
        var digits = new string(main.Where(char.IsAsciiDigit).ToArray());
        if (digits.Length < 7) return null;
        if (main.TrimStart().StartsWith('+')) return "+" + digits;
        if (digits.StartsWith("886")) return "+" + digits;
        return digits.StartsWith('0') ? "+886" + digits[1..] : digits;
    }

    // 只接受站內路徑（/upload/...）或 http(s) 網址，避免 javascript: 之類的值。
    private static bool IsImageUrl(string url)
    {
        if (url.StartsWith('/') && !url.StartsWith("//")) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static string Format(HtmlNode node, string value, int index)
    {
        var format = node.GetAttributeValue("data-edit-format", "");
        if (format.Length == 0) return value;
        return WebUtility.HtmlDecode(format).Replace("{index}", index.ToString()).Replace("{value}", value);
    }

    private static void SetText(HtmlNode node, string text)
    {
        node.RemoveAllChildren();
        if (text.Length > 0)
            node.AppendChild(node.OwnerDocument.CreateTextNode(Encode(text)));
    }

    private static void SetMultiline(HtmlNode node, string text)
    {
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
            .Select(line => Encode(line.TrimEnd()));
        node.InnerHtml = string.Join("<br/>", lines).Trim();
    }

    private static void SetAttr(HtmlNode node, string name, string value)
        => node.SetAttributeValue(name, Encode(value));

    // 文章存檔用 StringHandler.HtmlEncode（先解碼再編碼），文字裡的 &lt; 存回去會變成真的「<」。
    // 欄位值來自客戶上傳的 Excel，先把角括號換成全形，避免被當成標籤。
    private static string Encode(string value)
        => WebUtility.HtmlEncode(value.Replace('<', '＜').Replace('>', '＞'));

    private static void RemoveClass(HtmlNode node, string className)
    {
        var classes = node.GetAttributeValue("class", "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(c => c != className)
            .ToList();
        if (classes.Count == 0)
            node.Attributes.Remove("class");
        else
            node.SetAttributeValue("class", string.Join(" ", classes));
    }

    // alt 用的欄位名稱：去掉「（1000×605）」這類尺寸說明。
    private static string LabelText(HtmlNode node)
    {
        var label = node.GetAttributeValue("data-edit-label", "");
        var cut = label.IndexOfAny(['（', '(']);
        return (cut > 0 ? label[..cut] : label).Trim();
    }
}