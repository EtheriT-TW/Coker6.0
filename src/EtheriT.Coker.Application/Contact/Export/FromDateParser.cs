using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace EtheriT.Coker.Application.Contact.Export
{
    /// <summary>
    /// FromDate JSON 解析後的動態欄位定義。
    /// </summary>
    public class FromDateColumn
    {
        /// <summary>
        /// 規格要求以欄位 key 第一段做比對，避免同欄位因尾段資訊不同而重複。
        /// </summary>
        public string NormalizedKey { get; set; } = string.Empty;

        /// <summary>
        /// FromDate JSON 內原始欄位 key。
        /// </summary>
        public string OriginalKey { get; set; } = string.Empty;

        /// <summary>
        /// Excel 顯示用欄名，優先使用 FromDate 內的 title。
        /// </summary>
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// 單筆聯絡資料的 FromDate 解析結果。
    /// </summary>
    public class FromDateParsedRecord
    {
        /// <summary>
        /// 對應 Contacts.Id，後續組 Excel rows 時用來回填動態欄位值。
        /// </summary>
        public long ContactId { get; set; }

        /// <summary>
        /// key 為正規化欄位 key，value 為使用者送出的欄位值。
        /// </summary>
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// FromDate 批次解析結果，包含欄位聯集與每筆資料的值。
    /// </summary>
    public class FromDateParseResult
    {
        /// <summary>
        /// 依資料首次出現順序建立的動態欄位聯集。
        /// </summary>
        public List<FromDateColumn> Columns { get; set; } = new List<FromDateColumn>();

        /// <summary>
        /// 每筆聯絡資料的解析值，key 為 Contacts.Id。
        /// </summary>
        public Dictionary<long, FromDateParsedRecord> Records { get; set; } = new Dictionary<long, FromDateParsedRecord>();
    }

    /// <summary>
    /// 解析 Contacts.FromDate 的 title/value JSON，供匯出動態欄位使用。
    /// </summary>
    public class FromDateParser
    {
        // captcha 為驗證碼欄位，依規格固定排除，不輸出到 Excel。
        private static readonly HashSet<string> ExcludedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "captcha"
        };

        private readonly ILogger? logger;

        public FromDateParser(ILogger? logger = null)
        {
            this.logger = logger;
        }

        /// <summary>
        /// 解析多筆 FromDate，遇到格式異常時只記錄警告並保留該筆固定欄位。
        /// </summary>
        public FromDateParseResult Parse(IEnumerable<(long ContactId, string? FromDate)> source)
        {
            var result = new FromDateParseResult();
            var knownColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in source)
            {
                var record = new FromDateParsedRecord { ContactId = item.ContactId };
                result.Records[item.ContactId] = record;

                if (string.IsNullOrWhiteSpace(item.FromDate))
                {
                    continue;
                }

                try
                {
                    using var json = JsonDocument.Parse(item.FromDate);
                    if (json.RootElement.ValueKind != JsonValueKind.Object)
                    {
                        logger?.LogWarning("Contact export skipped non-object FromDate JSON. ContactId: {ContactId}", item.ContactId);
                        continue;
                    }

                    foreach (var property in json.RootElement.EnumerateObject())
                    {
                        // 欄位 key 用正規化結果去重，避免同一欄位產生多個 Excel 欄。
                        var normalizedKey = NormalizeKey(property.Name);
                        if (string.IsNullOrWhiteSpace(normalizedKey) || ExcludedKeys.Contains(normalizedKey))
                        {
                            continue;
                        }

                        var title = GetPropertyText(property.Value, "title");
                        var value = GetPropertyText(property.Value, "value");

                        // 動態欄位順序以查詢結果中首次出現的順序為準。
                        if (!knownColumns.Contains(normalizedKey))
                        {
                            result.Columns.Add(new FromDateColumn
                            {
                                NormalizedKey = normalizedKey,
                                OriginalKey = property.Name,
                                Title = string.IsNullOrWhiteSpace(title) ? property.Name : title
                            });
                            knownColumns.Add(normalizedKey);
                        }

                        // 同一筆資料若重複出現同 key，保留第一個值以符合欄位去重策略。
                        if (!record.Values.ContainsKey(normalizedKey))
                        {
                            record.Values[normalizedKey] = value;
                        }
                    }
                }
                catch (JsonException ex)
                {
                    logger?.LogWarning(ex, "Contact export failed to parse FromDate JSON. ContactId: {ContactId}", item.ContactId);
                }
            }

            return result;
        }

        /// <summary>
        /// 欄位 key 正規化；規格要求取 trim 後第一段空白分隔值。
        /// </summary>
        public static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            return key.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// 從 FromDate 欄位物件中取出 title 或 value 文字。
        /// </summary>
        private static string GetPropertyText(JsonElement element, string propertyName)
        {
            if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
            {
                return string.Empty;
            }

            return ToDisplayText(property);
        }

        /// <summary>
        /// 將 JSON 值轉為 Excel 可顯示文字。
        /// </summary>
        private static string ToDisplayText(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Array => string.Join(", ", element.EnumerateArray().Select(ToDisplayText)),
                JsonValueKind.Object => element.GetRawText(),
                _ => string.Empty
            };
        }
    }
}
