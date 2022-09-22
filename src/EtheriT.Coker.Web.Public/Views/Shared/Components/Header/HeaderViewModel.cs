using EtheriT.Coker.Web.Public.Views.Shared.Components.MenuItem;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Header
{
    public class HeaderViewModel
    {
        public string? Title { get; set; }
        public string? LogoImageUrl { get; set; }
        public List<MenuItemModel>? menuItemModels { get; set; }
    }
}
