using EtheriT.Coker.Application.Shared.Dto.Marquee;
using EtheriT.Coker.Web.Public.Views.Shared.Components.MenuItem;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.Footer
{
    public class FooterViewModel
    {
        public string? Title { get; set; }
        public string? Link { get; set; }
        public List<FooterViewModel>? footerViewModels { get; set; }
    }
}
