using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static class L
    {
        public static string local { get; set; } = "zh-tw";
        private static readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _cache = new(StringComparer.OrdinalIgnoreCase);
        public static string get(string key, params object?[] args)
        {
            if (string.IsNullOrWhiteSpace(key)) return string.Empty;

            var map = GetLocaleMap(local);
            if (!map.TryGetValue(key, out var val) || string.IsNullOrEmpty(val))
                return string.Empty;

            if (args == null || args.Length == 0) return val;

            try
            {
                return string.Format(CultureInfo.CurrentUICulture, val, args);
            }
            catch (FormatException)
            {
                return SafeFormat(val, args);
            }
        }
        public static string getAllJsonString()
        {
            var map = GetLocaleMap(local);

            // 產出「合法 JSON」，避免你目前 {Key:"Value",} 這種格式 :contentReference[oaicite:2]{index=2}
            return JsonSerializer.Serialize(map, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 若你有中文/特殊符號，這樣較好讀
            });
        }
        private static string SafeFormat(string template, object?[] args)
        {
            if (string.IsNullOrEmpty(template)) return template;

            // 先把跳脫大括號暫時換掉，避免被當作佔位
            const string L = "\uF000"; // 不常用字元做暫存
            const string R = "\uF001";
            string t = template.Replace("{{", L).Replace("}}", R);

            // 僅匹配 {數字}；若有人寫了 {0:format} 或 {0,10} 這類，這裡會當作普通文字不處理
            t = Regex.Replace(t, @"\{(\d+)\}", m =>
            {
                if (!int.TryParse(m.Groups[1].Value, out int idx)) return m.Value;
                if (idx < 0 || idx >= args.Length) return m.Value;
                return args[idx]?.ToString() ?? "";
            });

            // 還原跳脫大括號
            return t.Replace(L, "{").Replace(R, "}");
        }
        private static IReadOnlyDictionary<string, string> GetLocaleMap(string locale)
        {
            return _cache.GetOrAdd(locale, loc =>
            {
                var type = GetTypeByLocale(loc);
                // 把所有 public static string property 一次讀出來
                var dict = type.GetProperties(BindingFlags.Static | BindingFlags.Public)
                               .Where(p => p.PropertyType == typeof(string) && p.GetIndexParameters().Length == 0)
                               .ToDictionary(
                                   p => p.Name,
                                   p => (string)(p.GetValue(null) ?? string.Empty),
                                   StringComparer.OrdinalIgnoreCase
                               );

                // 如果你未來真的有 field，也可補上（目前你 Locale/LocaleEn 都是 property）
                foreach (var f in type.GetFields(BindingFlags.Static | BindingFlags.Public)
                                      .Where(f => f.FieldType == typeof(string)))
                {
                    dict[f.Name] = (string)(f.GetValue(null) ?? string.Empty);
                }

                return dict;
            });
        }
        private static Type GetTypeByLocale(string loc)
            => string.Equals(loc, "en", StringComparison.OrdinalIgnoreCase) ? typeof(LocaleEn) : typeof(Locale);

    }
}
