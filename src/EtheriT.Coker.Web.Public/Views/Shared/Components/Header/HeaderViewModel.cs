using EtheriT.Coker.Application.Shared.Dto.Marquee;
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
        public bool? Sitemap_Target { get; set; }
        public bool? HasShoppingCar { get; set; }
        public List<MenuItemModel>? menuItemModels { get; set; }
        public List<MenuItemModel> langMenuItemModels { get; set; } = new List<MenuItemModel>();
        public List<MarqueeDisplayDto>? marqueeModels { get; set; }
    }
}
