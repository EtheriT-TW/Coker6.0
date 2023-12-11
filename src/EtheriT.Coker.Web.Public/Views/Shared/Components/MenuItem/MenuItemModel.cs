namespace EtheriT.Coker.Web.Public.Views.Shared.Components.MenuItem
{
    public class MenuItemModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Link { get; set; }
        public bool? Target { get; set; }
        public string? imageUrl { get; set; }
        public string? hoverImageUrl { get; set; }
        public string? Icon { get; set; }
        public string? IconClass { get; set; }
        public string? ImageIcon { get; set; }
        public int Length { get; set; } = 0;
        public List<MenuItemModel>? menuItemModels { get; set; }
    }
}
