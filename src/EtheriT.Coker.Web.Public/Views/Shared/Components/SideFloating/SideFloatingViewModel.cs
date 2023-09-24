using EtheriT.Coker.Application.Shared.Dto.HtmlContent;

namespace EtheriT.Coker.Web.Public.Views.Shared.Components.SideFloating
{
    public class SideFloatingViewModel
    {
        public List<HtmlContentDisplayDto>? rightSideAd { get; set; }
        public bool Chatbot {get; set; }=false;
    }
}
