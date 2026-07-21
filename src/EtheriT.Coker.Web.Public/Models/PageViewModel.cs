using EtheriT.Coker.Application.Shared.Dto.Advertise;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Processor;
using EtheriT.Coker.Application.Shared.Dto.Freight;
using EtheriT.Coker.Application.Shared.Dto.HtmlContent;
using EtheriT.Coker.Application.Shared.Dto.Search;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using EtheriT.Coker.Application.Shared.Dto.ThirdParty;
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
        public string locale { get; set; } = "zh-tw";
        public string root { get; set; }
        public bool IsProduction { get; set; }
        public WebsiteLevelEnum Level { get; set; }
        public List<AdvertiseDisplayDto>? enterAd { get; set; }
        public GetFrontContenOutputDto? ParentData { get; set; }
        public GetFrontContenOutputDto? PageData { get; set; }
        public string SafeHtml { get; set; } = string.Empty;
        public string SafeCss { get; set; } = string.Empty;
        public string ParentSafeHtml { get; set; } = string.Empty;
        public string ParentSafeCss { get; set; } = string.Empty;
        public long HtmlSanitizeWebsiteId { get; set; }
        public HtmlSanitizeSourceType HtmlSanitizeSourceType { get; set; } = HtmlSanitizeSourceType.頁面;
        public long HtmlSanitizeSourceId { get; set; }
        public long ParentHtmlSanitizeSourceId { get; set; }
        public string HtmlSanitizeContentKey { get; set; } = "Published";
        public string HtmlSanitizePolicy { get; set; } = "PublicHtml";
        public bool RewriteUploadPaths { get; set; }
        public string UploadOrgName { get; set; } = string.Empty;
        public string UploadParentOrgNames { get; set; } = string.Empty;
        public FrontSearchPalameterDro? SearchPalameter { get; set; }
        public List<FreightDisplayDto>? freightModels { get; set; }
        public List<PaymentTypeItemOutputDto>? paymentModels { get; set; }
        public List<GetMenuBreadDto>? MenuBread { get; set; }
        public StoreSetFrontDto storeSet { get; set; }
    }
}
