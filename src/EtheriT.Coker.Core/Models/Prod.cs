using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class Prod : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(150)]
        public string Title { get; set; }
        public int Ser_No { get; set; }
        [StringLength(100)]
        public string? ItemNo { get; set; }
        [StringLength(3000)]
        public string Introduction { get; set; }
        [StringLength(3000)]
        public string Description { get; set; }
        public double? Discount { get; set; }
        public virtual DateTime? StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public bool permanent { get; set; }
        public bool Visible { get; set; }
        public bool RemovedFromShelves { get; set; }
        public ProdStatusEnum Status { get; set; }
        public int? Clicks { get; set; }
        public string? SaveHtml { get; set; }
        [MaxLength]
        public string? SaveCss { get; set; }
        [MaxLength]
        public string? Html { get; set; }
        [MaxLength]
        public string? Css { get; set; }
        public Website? Website { get; set; }
        public List<Prod_Stock> Prod_Stocks { get; set; }
        public List<Prod_Log> Prod_Logs { get; set; }
        public List<Remote> Remotes { get; set; }
        public List<Prod_TechCert> TechnicalCertificates { get; set; }
        public List<MappingLogisticsSettingAndProd> MappingLogisticsSettingAndProds { get; set; }
    }
}
