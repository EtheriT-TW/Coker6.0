using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Web.Public.Views.Shared.Components.MenuItem;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.LayoutType
{
    public class LayoutTypeViewModel
    {
        public string? SiteName { get; set; }
        public long? SiteId { get; set; }
        public string? OrgName { get; set; }
        public int? LayoutType { get; set; }
        public bool? IsFaPage { get; set; }
    }
}
