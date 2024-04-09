using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_TechCert : FullAuditedEntity
    {
        public long FK_PId { get; set; }
        public long FK_TCId { get; set; }
        public bool IsChecked { get; set; }
        public TechnicalCertificate TechnicalCertificate { get; set; }
        public Prod Prod { get; set; }
    }
}
