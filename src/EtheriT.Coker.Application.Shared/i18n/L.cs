using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.i18n
{
    public static class L
    {
        public static string local { get; set; } = "zh-tw";
        public static string get(string key) {
            string className;
            string val = string.Empty;
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
            try {
                object? o = GetStaticMemberValue(type, key);
                if (o != null) val = o.ToString()??"";
            }
            catch(ArgumentException e) {
                val = "";
            }
            return val;
        }
        public static string getAllJsonString()
		{
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
    }
}
