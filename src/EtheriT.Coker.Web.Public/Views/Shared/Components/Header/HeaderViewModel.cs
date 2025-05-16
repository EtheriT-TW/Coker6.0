using DevExpress.XtraCharts.Native;
using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Application.Shared.Dto.Templates;
using EtheriT.Coker.Web.Public.Views.Shared.Components.MenuItem;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Header
{
    public class HeaderViewModel
    {
        public string? Title { get; set; }
        public string? HomeLink { get; set; }
        public bool? HomeTarget { get; set; }
        public string? LogoImageUrl { get; set; }
        public string? Sitemap_Link { get; set; }
        public string? SearchPath { get; set; }
        public string? UploadPath { get; set; }
        public bool? Sitemap_Target { get; set; }
        public bool? HasShoppingCar { get; set; }
        public bool ShowPagePath { get; set; } = true;
        public bool ShowMarquee { get; set; } = true;
        public string marqueeBagroundImage { get; set; } = string.Empty;
        public string marqueeIcon { get; set; } = string.Empty;
        public TemplatesDto? templates { get; set; }
        public List<SliderDto> Bannners { get; set; } = new List<SliderDto>();
        public List<MenuItemModel>? menuItemModels { get; set; }
        public List<MenuItemModel> langMenuItemModels { get; set; } = new List<MenuItemModel>();
        public List<MarqueeDisplayDto>? marqueeModels { get; set; }
    }
}
