namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class GlobalSettingsDto
    {
        public int SchemaVersion { get; set; } = 1;
        public GlobalVisibilitySettingsDto Visibility { get; set; } = new GlobalVisibilitySettingsDto();
        public const int MinMarqueeDelaySeconds = 1;
        public const int MaxMarqueeDelaySeconds = 60;
        public int MarqueeDelaySeconds { get; set; } = 3;
    }

    public class GlobalVisibilitySettingsDto
    {
        public bool ShowMarquee { get; set; } = true;
        public bool ShowPagePath { get; set; } = true;
        public bool ShowPopular { get; set; } = false;
        public bool ShowPublishDate { get; set; } = true;
    }
}
