using Newtonsoft.Json;

namespace EtheriT.Coker.Application.ArticleTemplates;

// 樣板欄位資料：{"欄位key":["值",...]}。單值欄位只用第一個值，List／Gallery 用全部。
public static class ArticleFieldValues
{
    // 表單常見的「沒有」寫法，視同空白（樣板會把整列隱藏）。
    private static readonly HashSet<string> BlankWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "無", "nil", "null", "none", "n/a", "-"
    };

    public static Dictionary<string, List<string>> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, List<string>>(StringComparer.Ordinal);

        try
        {
            var parsed = JsonConvert.DeserializeObject<Dictionary<string, List<string>?>>(json);
            var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            if (parsed == null) return result;
            foreach (var (key, values) in parsed)
                result[key] = Clean(values);
            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("文章欄位資料格式不正確。", ex);
        }
    }

    public static string Serialize(IReadOnlyDictionary<string, List<string>> values)
        => JsonConvert.SerializeObject(values.ToDictionary(e => e.Key, e => Clean(e.Value), StringComparer.Ordinal));

    public static List<string> Clean(IEnumerable<string?>? values)
        => (values ?? [])
            .Select(v => (v ?? "").Trim())
            .Where(v => v.Length > 0 && !BlankWords.Contains(v))
            .ToList();

    public static IReadOnlyList<string> All(IReadOnlyDictionary<string, List<string>> values, string key)
        => values.TryGetValue(key, out var list) ? Clean(list) : [];

    public static string First(IReadOnlyDictionary<string, List<string>> values, string key)
        => All(values, key).FirstOrDefault() ?? "";
}