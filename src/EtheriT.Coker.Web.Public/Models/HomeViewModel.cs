using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Product;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;

namespace EtheriT.Coker.Web.Public.Models
{
    public class HomeViewModel
    {
        public List<HtmlContentDisplayDto>? enterAd { get; set; }
        public List<ProdGetDisplayDto>? guessLike { get; set; }
        public String site_name { get; set; }
		public String OrgName { get; set; }
        public string layout { get; set; }
        public string? token { get; set; }
        public string locale { get; set; } = "zh-tw";
        public string PageView { get; set; }
        public WebsiteLevelEnum Level { get; set; }
		public GetFrontContenOutputDto? PageData { get; set; }
        public List<GetMenuBreadDto>? MenuBread { get; set; }
        public StoreSetFrontDto storeSet { get; set; }
    }
}
