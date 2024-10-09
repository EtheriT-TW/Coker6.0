using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Website : FullAuditedEntity
    {
        public string? DefaultUrl { get; set; }
        public string Title { get; set; }
        public string OrgName { get; set; }
        public string? Description { get; set; }
        public string? Contact { get; set; }
        public string? Icon { get; set; }
        public string? Logo { get; set; }
        public string Locale { get; set; }
        public string Type { get; set; }
        public int? LayoutType { get; set; }
        public string? Keywords { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public string? Statement { get; set; }
        public string? Contract { get; set; }
        public int Level{ get; set; }
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
    }
}
