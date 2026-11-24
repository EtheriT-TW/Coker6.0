using EtheriT.Coker.Application.Shared.Dto.ArticleTemplate;
using HtmlAgilityPack;
using System.Collections.Concurrent;

namespace EtheriT.Coker.Application.ArticleTemplates;

public sealed class ArticleTemplate
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Html { get; init; }
    public required string Css { get; init; }
    public required string PageTitleKey { get; init; }
    public required IReadOnlyList<ArticleTemplateFieldDto> Fields { get; init; }
}

// 目錄文章樣板寫在程式裡（不存資料庫）。
// 新增樣板：在 Resources/ArticleTemplates 放 {key}.html 與 {key}.css，再到 Definitions 加一行。
// 屬性規格見「文章版型功能.pptx」，另加 data-edit-empty、Map、List（2026-11-23 定案）。
public static class ArticleTemplateCatalog
{
    private const string ResourcePrefix = "EtheriT.Coker.Application.ArticleTemplates.";

    private static readonly ArticleTemplateInfoDto[] Definitions =
    [
        new("vendor-profile", "廠商介紹", "公司主視覺、簡介、營運項目、Logo 圖片與廠商資訊（含地圖）")
    ];

    private static readonly ConcurrentDictionary<string, Lazy<ArticleTemplate>> Cache =
        new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyList<ArticleTemplateInfoDto> List() => Definitions;

    public static bool Exists(string? key)
        => !string.IsNullOrWhiteSpace(key)
            && Definitions.Any(d => string.Equals(d.Key, key.Trim(), StringComparison.OrdinalIgnoreCase));

    public static ArticleTemplate Get(string key)
    {
        var definition = Definitions.FirstOrDefault(d =>
                string.Equals(d.Key, key?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException($"找不到文章樣板「{key}」。");
        return Cache.GetOrAdd(definition.Key, _ => new Lazy<ArticleTemplate>(() => Load(definition))).Value;
    }

    // 換樣板時：舊樣板有、新樣板沒有的欄位，這些資料換過去不會顯示。
    public static IReadOnlyList<ArticleTemplateFieldDto> MissingFields(string fromKey, string toKey)
    {
        var target = Get(toKey).Fields.Select(f => f.Key).ToHashSet(StringComparer.Ordinal);
        return Get(fromKey).Fields.Where(f => !target.Contains(f.Key)).ToList();
    }

    internal static HtmlDocument ParseHtml(string html)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        return document;
    }

    internal static string EditType(HtmlNode node)
        => node.GetAttributeValue("data-edit-type", "").Trim().ToLowerInvariant();

    internal static string EditKey(HtmlNode node)
        => node.GetAttributeValue("data-edit-key", "").Trim();

    internal static IEnumerable<HtmlNode> KeyedNodes(HtmlNode root)
        => root.Descendants().Where(n => n.NodeType == HtmlNodeType.Element && EditKey(n).Length > 0);

    // List／Gallery 的子項目範本：data-edit-template 指向欄位內的元素 id。
    internal static HtmlNode? ItemTemplate(HtmlNode field)
    {
        var id = field.GetAttributeValue("data-edit-template", "").Trim().TrimStart('#');
        return id.Length == 0 ? null : field.Descendants().FirstOrDefault(n => n.Id == id);
    }

    private static ArticleTemplate Load(ArticleTemplateInfoDto definition)
    {
        var html = ReadResource(definition.Key + ".html");
        var css = ReadResource(definition.Key + ".css");
        var errors = new List<string>();
        var fields = ReadFields(ParseHtml(html), errors);

        var pageTitles = fields.Where(f => f.Type == ArticleFieldTypes.PageTitle).ToList();
        if (pageTitles.Count != 1)
            errors.Add($"PageTitle 欄位必須剛好一個（目前 {pageTitles.Count} 個）");

        if (errors.Count > 0)
            throw new InvalidOperationException(
                $"文章樣板「{definition.Key}」格式錯誤：{string.Join("；", errors)}");

        return new ArticleTemplate
        {
            Key = definition.Key,
            Name = definition.Name,
            Html = html,
            Css = css,
            PageTitleKey = pageTitles[0].Key,
            Fields = fields
        };
    }

    private static List<ArticleTemplateFieldDto> ReadFields(HtmlDocument document, List<string> errors)
    {
        var fields = new List<ArticleTemplateFieldDto>();
        var sortKeys = new Dictionary<string, int>(StringComparer.Ordinal);
        var keys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in KeyedNodes(document.DocumentNode))
        {
            var key = EditKey(node);
            var type = EditType(node);
            if (!keys.Add(key))
                errors.Add($"data-edit-key「{key}」重複");
            if (!ArticleFieldTypes.Supported.Contains(type))
            {
                errors.Add($"欄位「{key}」的 data-edit-type「{type}」目前不支援");
                continue;
            }
            if (type is ArticleFieldTypes.Section or ArticleFieldTypes.Map)
                continue;
            if ((type is ArticleFieldTypes.List or ArticleFieldTypes.Gallery) && ItemTemplate(node) == null)
                errors.Add($"欄位「{key}」的 data-edit-template 找不到對應 id（範本必須放在欄位裡面）");

            // data-edit-order：畫面上看到的順序與 HTML 順序不同時（例如 CSS order、直書），用它指定欄位排序；
            // 沒寫的欄位排在有寫的後面，維持 HTML 順序。預覽頁編輯彈窗依此順序顯示。
            sortKeys[key] = int.TryParse(node.GetAttributeValue("data-edit-order", ""), out var order)
                ? order
                : 1000 + fields.Count;
            fields.Add(new ArticleTemplateFieldDto(
                key,
                NullIfEmpty(node.GetAttributeValue("data-edit-label", "")) ?? key,
                type,
                string.Equals(node.GetAttributeValue("data-edit-required", ""), "true", StringComparison.OrdinalIgnoreCase),
                SectionLabel(node),
                int.TryParse(node.GetAttributeValue("data-edit-maxcount", ""), out var max) && max > 0 ? max : null));
        }

        var fieldKeys = fields.Select(f => f.Key).ToHashSet(StringComparer.Ordinal);
        foreach (var map in document.DocumentNode.Descendants().Where(n => EditType(n) == ArticleFieldTypes.Map))
        {
            var reference = map.GetAttributeValue("data-edit-ref", "").Trim();
            if (!fieldKeys.Contains(reference))
                errors.Add($"Map 的 data-edit-ref「{reference}」不是樣板內的欄位");
        }

        return fields.OrderBy(f => sortKeys[f.Key]).ToList();
    }

    private static string? SectionLabel(HtmlNode node)
    {
        var section = node.Ancestors().FirstOrDefault(a => EditType(a) == ArticleFieldTypes.Section);
        return section == null ? null : NullIfEmpty(section.GetAttributeValue("data-edit-label", ""));
    }

    private static string? NullIfEmpty(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string ReadResource(string fileName)
    {
        using var stream = typeof(ArticleTemplateCatalog).Assembly.GetManifestResourceStream(ResourcePrefix + fileName)
            ?? throw new FileNotFoundException($"內嵌文章樣板不存在：{fileName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}

public static class ArticleFieldTypes
{
    public const string PageTitle = "pagetitle";
    public const string String = "string";
    public const string Textarea = "textarea";
    public const string Link = "link";
    public const string List = "list";
    public const string Image = "image";
    public const string Gallery = "gallery";
    public const string Map = "map";
    public const string Section = "section";
    public const string Tags = "tags";

    // pptx 其餘型別（Email、Number、Date、LinkList、Tags、Files、Slider…）尚未實作，樣板用到會在載入時報錯。
    public static readonly IReadOnlySet<string> Supported = new HashSet<string>(StringComparer.Ordinal)
    {
        PageTitle, String, Textarea, Link, List, Image, Gallery, Map, Section
    };
}