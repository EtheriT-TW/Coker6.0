using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Web.Public.Views.Shared.Components.MenuItem;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Footer
{
    public class FooterViewModel
    {
        public string? Title { get; set; }
        public string? Link { get; set; }
        public string? line_qr { get; set; }
        public string? line_title { get; set; }
        public string? line_describe { get; set; }
        public string? Sitemap_Link { get; set; }
        public string? Privacy_Link { get; set; }
        public string? Accessibility_Link { get; set; }
        public string? Accessibility_Badge { get; set; }
        public string? Logo_Image { get; set; }
        public string? LINE_Link { get; set; }
        public string? Facebook_Link { get; set; }
        public string? IG_Link { get; set; }
        public string? YoutubeChannel_Link { get; set; }
        public string locale { get; set; } = "zh-tw";
        public List<string>? Content { get; set; } = new List<string>();
        public List<FooterViewModel>? footerViewModels { get; set; }
        public string? Copyright { get; set; }
    }
}
