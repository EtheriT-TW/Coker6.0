using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public static class GlobalSettingsResolver
    {
        public static GlobalSettingsDto Resolve(TemplatesDto? template)
        {
            var headerContentConfig = template?.templateSections
                .FirstOrDefault(x => x.sectionType == SectionTypeEnum.表頭)
                ?.ContentConfig;

            return Resolve(template?.LayoutConfig, headerContentConfig);
        }

        public static GlobalSettingsDto Resolve(string? layoutConfig, string? headerContentConfig)
        {
            var settings = new GlobalSettingsDto();
            var hasVisibilitySettings = false;

            if (!string.IsNullOrWhiteSpace(layoutConfig))
            {
                try
                {
                    var json = JObject.Parse(layoutConfig);
                    hasVisibilitySettings = json.GetValue(
                        nameof(GlobalSettingsDto.Visibility),
                        StringComparison.OrdinalIgnoreCase) != null;

                    settings = json.ToObject<GlobalSettingsDto>() ?? settings;
                    settings.Visibility ??= new GlobalVisibilitySettingsDto();
                }
                catch (JsonException)
                {
                    // 無效或舊格式的 LayoutConfig 視為尚未建立全站設定，
                    // 繼續使用 Header JSON 相容值。
                }
            }

            if (!hasVisibilitySettings && !string.IsNullOrWhiteSpace(headerContentConfig))
            {
                try
                {
                    var legacy = JsonConvert.DeserializeObject<HeaderContentConfigDto>(headerContentConfig);
                    if (legacy != null)
                    {
                        settings.Visibility.ShowMarquee = legacy.ShowMarquee;
                        settings.Visibility.ShowPagePath = legacy.ShowPagePath;
                    }
                }
                catch (JsonException)
                {
                    // Header JSON 無法解析時保留安全預設值。
                }
            }

            return settings;
        }
    }
}
