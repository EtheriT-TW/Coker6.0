using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class Website : FullAuditedEntity
    {
		[StringLength(255)]
		public string? DefaultUrl { get; set; }
		[StringLength(250)]
		public string Title { get; set; }
		[StringLength(100)]
		public string OrgName { get; set; }
        public string? Description { get; set; }
        public string? Contact { get; set; }
        public string? Icon { get; set; }
        public string? Logo { get; set; }
		[StringLength(10)]
		public string Locale { get; set; }
		[StringLength(10)]
		public string Type { get; set; }
        public int? LayoutType { get; set; }
        public string? Keywords { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public string? Statement { get; set; }
		[StringLength(200)]
		public string? Contract { get; set; }
		public string? Css { get; set; }
        [StringLength(200)]
        public string? ContactMail { get; set; }
        public WebsiteLevelEnum Level { get; set; }

        // ── 客戶管理平台（Web.Platform）維護的欄位 ──
        // 後台與前台都不讀寫這些欄位；不可加進 WebsiteEditDto，否則後台「網站資料」頁存檔會把它們洗掉。
        public PlatformWebsiteStatusEnum ServiceStatus { get; set; }    // 正常／暫停／註銷（目前僅為註記，不影響前後台）
        public DateTime? TerminatedDate { get; set; }                   // 註銷日期
        [StringLength(200)]
        public string? HostLocation { get; set; }                       // 主機位置
        public bool IsDomainPending { get; set; }                       // 網域待申請
        public long? FK_PlatformDomainId { get; set; }                  // 依網址比對出的網域；待申請或未填網址時為 null
        [StringLength(2000)]
        public string? PlatformRemark { get; set; }                     // 平台備註
        public PlatformDomain? PlatformDomain { get; set; }
        public List<MappingUserAndWebsite> Users { get; set; }
        public List<MappingFrontUserAndWebsite> FrontUsers { get; set; }
        public List<Marquee> Marquees { get; set; }
        public List<WebMenu> WebMenus { get; set; }
        public List<Prod> Prods { get; set; }
        public List<Prod_Spec_Type> Prod_Spec_Types { get; set; }
        public List<Html_Content> Html_Contents { get; set; }
        public List<LogisticsSetting> LogisticsSettings { get; set; }
        public List<TechnicalCertificate> TechnicalCertificates { get; set; }
        public List<Tag> Tags { get; set; }
        public List<FileUpload> Files { get; set; }
        public List<Advertise> Advertise { get; set; }
        public List<Article> Articles { get; set; }
        public List<Directory> Directory { get; set; }
        public List<StoreSetDetail> StoreSetDetails { get; set; }
        public List<CustSearch> CustSearchs { get; set; }
        public List<AuditLog> AuditLogs { get; set; }
        public List<MappingCompanyAndWebsites> Company { get; set; }
        public List<Recipient>? Recipients { get; set; }
        public List<Permissions> Permissions { get; set; }
        public List<PermissionDetail> PermissionDetails { get; set; }
        public List<NotFoundImage> NotFoundImages { get; set; }
        public List<JsonObject> jsonObjects { get; set; }
        public List<SearchLog> SearchLogs { get; set; }
        public List<ThirdPartyKeypairValue> thirdPartyKeypairValues { get; set; }
        public List<PaymentTypesValue> paymentTypesValues { get; set; }
        public List<Template> Templates { get; set; }
        public List<FlowSize> flowSizes { get; set; }
        public ICollection<LogisticsBox> logisticsBoxes { get; set; }
        public ICollection<WebsiteCacheState> websiteCacheStates { get; set; }
        public ICollection<HtmlSanitizeState> htmlSanitizeStates { get; set; }
        public virtual ICollection<MarketingCampaign> MarketingCampaigns { get; set; } = new List<MarketingCampaign>();
    }
}
