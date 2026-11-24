using EtheriT.Coker.Application.ArticleTemplates;

namespace EtheriT.Coker.Application.ArticleImports;

// 套用圖片規則用：這列下載成功的一張圖片。
public sealed record ArticleImportImageChoice(string Path, string Column, string FileName);

// AI 填好或使用者在預覽頁修改的欄位資料，存檔前一律經過這裡：
// 只留樣板有的欄位、依型別限制筆數與長度、圖片只能用這列下載的檔案、必填欄位要有值。
public static class ArticleImportFieldRules
{
    public const int MaxTitleLength = 150;      // Article.Title 的長度上限
    public const int MaxStringLength = 300;
    public const int MaxLinkLength = 500;
    public const int MaxTextareaLength = 4000;
    public const int MaxListItems = 50;
    public const int MaxListItemLength = 100;
    public const int DefaultGalleryMax = 10;

    // resolveImage：把欄位值轉成圖片路徑，不是這列的圖片就回傳 null（AI 用代號 image:1，預覽頁送回路徑）。
    // images：這列下載成功的圖片（依 Excel 順序），用來套用圖片規則；不給就不套用。
    // byUser：使用者在預覽頁手動選的，允許同一張圖同時是官網橫幅與 Logo（AI 自動選時才排除）。
    public static (Dictionary<string, List<string>> Fields, string? Error) Normalize(
        ArticleTemplate template,
        IReadOnlyDictionary<string, List<string>>? raw,
        Func<string, string?> resolveImage,
        IReadOnlyList<ArticleImportImageChoice>? images = null,
        bool byUser = false)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        raw ??= new Dictionary<string, List<string>>();

        foreach (var field in template.Fields)
        {
            if (!raw.TryGetValue(field.Key, out var input)) continue;
            var values = ArticleFieldValues.Clean(input);
            var cleaned = field.Type switch
            {
                ArticleFieldTypes.PageTitle => Single(values, MaxTitleLength),
                ArticleFieldTypes.String => Single(values, MaxStringLength),
                ArticleFieldTypes.Link => Single(values, MaxLinkLength),
                ArticleFieldTypes.Textarea => Single(values, MaxTextareaLength),
                ArticleFieldTypes.List => values
                    .Select(v => Cut(OneLine(v), MaxListItemLength))
                    .Distinct(StringComparer.Ordinal)
                    .Take(MaxListItems)
                    .ToList(),
                ArticleFieldTypes.Image => Images(values, resolveImage).Take(1).ToList(),
                ArticleFieldTypes.Gallery => Images(values, resolveImage).Take(field.MaxCount ?? DefaultGalleryMax).ToList(),
                _ => []
            };
            if (cleaned.Count > 0) result[field.Key] = cleaned;
        }

        if (images is { Count: > 0 })
            ApplyImageRules(template, result, images, excludeFromGallery: !byUser);

        var missing = template.Fields
            .Where(f => (f.Required || f.Key == template.PageTitleKey) && !result.ContainsKey(f.Key))
            .Select(f => f.Label)
            .ToList();
        return (result, missing.Count == 0 ? null : $"缺少必填欄位：{string.Join("、", missing)}");
    }

    // 圖片規則（使用者 2026-11-24 定案）：
    // 1. Image 欄位（例如官網橫幅）只要這列有圖片就一定要有圖；AI 沒選時依序挑：
    //    欄名／檔名像橫幅的 → 沒被 Gallery 用到的 → 第一張。只有一張圖時就給 Image。
    // 2. AI 自動選時，Image 用掉的圖不會出現在 Gallery（例如公司 Logo＋品牌 Logo），Gallery 可以是空的；
    //    使用者手動選則不排除（excludeFromGallery = false）。
    private static void ApplyImageRules(
        ArticleTemplate template,
        Dictionary<string, List<string>> result,
        IReadOnlyList<ArticleImportImageChoice> images,
        bool excludeFromGallery)
    {
        var galleryKeys = template.Fields.Where(f => f.Type == ArticleFieldTypes.Gallery).Select(f => f.Key).ToList();
        var usedBySingle = new HashSet<string>(StringComparer.Ordinal);

        foreach (var field in template.Fields.Where(f => f.Type == ArticleFieldTypes.Image))
        {
            if (!result.TryGetValue(field.Key, out var current) || current.Count == 0)
            {
                var inGallery = galleryKeys
                    .SelectMany(key => result.GetValueOrDefault(key) ?? [])
                    .ToHashSet(StringComparer.Ordinal);
                var free = images.Where(i => !usedBySingle.Contains(i.Path)).ToList();
                var pick = free.FirstOrDefault(LooksLikeBanner)
                    ?? free.FirstOrDefault(i => !inGallery.Contains(i.Path))
                    ?? free.FirstOrDefault();
                if (pick == null) continue;
                result[field.Key] = [pick.Path];
            }
            usedBySingle.Add(result[field.Key][0]);
        }

        if (!excludeFromGallery) return;
        foreach (var key in galleryKeys)
        {
            if (!result.TryGetValue(key, out var values)) continue;
            var kept = values.Where(v => !usedBySingle.Contains(v)).ToList();
            if (kept.Count == 0) result.Remove(key);
            else result[key] = kept;
        }
    }

    private static bool LooksLikeBanner(ArticleImportImageChoice image)
    {
        var text = $"{image.Column} {image.FileName}";
        return !text.Contains("logo", StringComparison.OrdinalIgnoreCase)
            && (text.Contains("橫幅") || text.Contains("banner", StringComparison.OrdinalIgnoreCase));
    }

    private static List<string> Single(List<string> values, int max)
        => values.Count == 0 ? [] : [Cut(values[0], max)];

    private static IEnumerable<string> Images(List<string> values, Func<string, string?> resolveImage)
        => values.Select(resolveImage).OfType<string>().Distinct(StringComparer.Ordinal);

    private static string OneLine(string value)
        => string.Join(" ", value.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string Cut(string value, int max) => value.Length > max ? value[..max].TrimEnd() : value;
}