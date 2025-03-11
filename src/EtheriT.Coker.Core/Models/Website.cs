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
		public WebsiteLevelEnum Level { get; set; }
        public List<MappingUserAndWebsite> Users { get; set; }
        public List<MappingFrontUserAndWebsite> FrontUsers { get; set; }
        public List<Marketing> Marketing { get; set; }
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
        public List<Theme> Themes { get; set; }
        public List<FlowSize> flowSizes { get; set; }
    }
}
