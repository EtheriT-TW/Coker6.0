using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static class L
    {
        public static string local { get; set; } = "zh-tw";
        public static string get(string key, params object?[] args) {
            string val = string.Empty;
            Type type = setType();
            try {
                object? o = GetStaticMemberValue(type, key);
                if (o != null) val = o.ToString()??"";
            }
            catch(ArgumentException e) {
                val = "";
            }
            if (string.IsNullOrEmpty(val) || args == null || args.Length == 0)
                return val;

            try
            {
                return string.Format(CultureInfo.CurrentUICulture, val, args);
            }
            catch (FormatException)
            {
                // 若模板或參數不匹配，就走安全替換（僅替換 {數字}）
                return SafeFormat(val, args);
            }
        }
        public static string getAllJsonString()
		{
			Type type = setType();
            return jsonconvert(type);
		}
        private static string jsonconvert(Type type) {
            string json = "{";
            var list = type.GetProperties();
			foreach (PropertyInfo property in list)
            {
                var p = type.GetProperty(property.Name, BindingFlags.Static | BindingFlags.Public);
                object value = "";
                if (p != null) value = p.GetValue(null)??"";
				json += $"{property.Name}:\"{value}\",";
			}
            json += "}";
            return json;
		}
        private static Type setType() {
            Type type;
            switch (local)
            {
                case "en":
                    type = typeof(LocaleEn);
                    break;
                default:
                    type = typeof(Locale);
                    break;
            }
            return type;
        }

		private static object? GetStaticMemberValue(Type type, string memberName)
        {
            // 先嘗試取得屬性
            PropertyInfo? property = type.GetProperty(memberName, BindingFlags.Static | BindingFlags.Public);
            if (property != null)
            {
                return property.GetValue(null);
            }

            // 如果屬性未找到，嘗試取得字段
            FieldInfo? field = type.GetField(memberName, BindingFlags.Static | BindingFlags.Public);
            if (field != null)
            {
                return field.GetValue(null);
            }

            throw new ArgumentException($"成員 '{memberName}' 未找到於類別 '{type.Name}'");
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
    }
}
