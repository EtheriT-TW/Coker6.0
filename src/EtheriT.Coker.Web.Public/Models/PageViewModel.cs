using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.WebMenu;

namespace EtheriT.Coker.Web.Public.Models
{
	public class PageViewModel
	{
		public long? id { get; set; }
        public string? option { get; set; }
        public string? search { get; set; }
        public string? orgName { get; set; }
        public string layout { get; set; }
		public string? token { get; set; }
        public WebsiteLevelEnum Level { get; set; }
		public List<HtmlContentDisplayDto>? enterAd { get; set; }
		public GetFrontContenOutputDto? ParentData { get; set; }
        public GetFrontContenOutputDto? PageData { get; set; }
        public FrontSearchPalameterDro? SearchPalameter { get; set; }
        public List<FreightDisplayDto>? freightModels { get; set; }
        public List<GetMenuBreadDto>? MenuBread { get; set; }
        public StoreSetFrontDto storeSet { get; set; }
    }
}
