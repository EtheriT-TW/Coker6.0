using MiniExcelLibs;
using System.Globalization;

namespace EtheriT.Coker.Application.ArticleImports;

public sealed record ArticleImportSheetRow(int RowNumber, IReadOnlyDictionary<string, string> Values);

public sealed record ArticleImportSheet(IReadOnlyList<string> Columns, IReadOnlyList<ArticleImportSheetRow> Rows);

// 讀客戶上傳的 Excel（第一個工作表）：第 1 列是欄名，第 2 列起每列一筆資料。
// 欄名空白的欄位略過；欄名重複時後面的加上「（2）」區分；整列空白略過。
public static class ArticleImportExcelReader
{
    public const int MaxRows = 1000;
    public const int MaxColumns = 200;
    public const int MaxCellLength = 4000;
    public const long MaxFileBytes = 20L * 1024 * 1024;

    public static ArticleImportSheet Read(Stream stream)
    {
        IEnumerable<IDictionary<string, object?>> query;
        try
        {
            query = MiniExcel.Query(stream, useHeaderRow: false, excelType: ExcelType.XLSX)
                .Cast<IDictionary<string, object?>>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("無法讀取 Excel 檔案，請確認是 .xlsx 格式。", ex);
        }

        using var enumerator = SafeEnumerate(query).GetEnumerator();
        if (!enumerator.MoveNext())
            throw new InvalidOperationException("Excel 沒有任何資料。");

        // Excel 欄位代號（A、B…）→ 欄名
        var headers = ReadHeaders(enumerator.Current);
        if (headers.Count == 0)
            throw new InvalidOperationException("Excel 第一列沒有欄名。");
        if (headers.Count > MaxColumns)
            throw new InvalidOperationException($"欄位超過 {MaxColumns} 個，請刪除不需要的欄位後再上傳。");

        var rows = new List<ArticleImportSheetRow>();
        var rowNumber = 1;
        while (enumerator.MoveNext())
        {
            rowNumber++;
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var (letter, name) in headers)
            {
                var text = enumerator.Current.TryGetValue(letter, out var raw) ? ToText(raw) : "";
                if (text.Length > 0) values[name] = text;
            }
            if (values.Count == 0) continue;
            if (rows.Count >= MaxRows)
                throw new InvalidOperationException($"資料超過 {MaxRows} 列，請分成多個檔案匯入。");
            rows.Add(new ArticleImportSheetRow(rowNumber, values));
        }

        return new ArticleImportSheet(headers.Select(h => h.Value).ToList(), rows);
    }

    // 欄名含「不公開」的欄位（例：聯絡人手機（不公開））預設不送給 AI。
    public static bool IsSelectedByDefault(string column) => !column.Contains("不公開", StringComparison.Ordinal);

    private static List<KeyValuePair<string, string>> ReadHeaders(IDictionary<string, object?> row)
    {
        var headers = new List<KeyValuePair<string, string>>();
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (letter, raw) in row)
        {
            var name = ToText(raw).Replace("\r", " ").Replace("\n", " ").Trim();
            if (name.Length == 0) continue;
            if (name.Length > 200) name = name[..200];
            var unique = name;
            for (var i = 2; !used.Add(unique); i++)
                unique = $"{name}（{i}）";
            headers.Add(new KeyValuePair<string, string>(letter, unique));
        }
        return headers;
    }

    private static string ToText(object? value)
    {
        var text = value switch
        {
            null => "",
            DateTime date when date.TimeOfDay == TimeSpan.Zero => date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture),
            DateTime date => date.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
            double number => number.ToString(CultureInfo.InvariantCulture),
            decimal number => number.ToString(CultureInfo.InvariantCulture),
            bool flag => flag ? "TRUE" : "FALSE",
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""
        };
        text = text.Trim();
        return text.Length > MaxCellLength ? text[..MaxCellLength] : text;
    }

    // MiniExcel 是邊讀邊回傳，格式錯誤會在列舉時才丟例外。
    private static IEnumerable<IDictionary<string, object?>> SafeEnumerate(IEnumerable<IDictionary<string, object?>> source)
    {
        using var enumerator = source.GetEnumerator();
        while (true)
        {
            try
            {
                if (!enumerator.MoveNext()) yield break;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                throw new InvalidOperationException("無法讀取 Excel 檔案，請確認是 .xlsx 格式。", ex);
            }
            yield return enumerator.Current;
        }
    }
}