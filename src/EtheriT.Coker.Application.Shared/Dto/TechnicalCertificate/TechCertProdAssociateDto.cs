
namespace EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate
{
    public class TechCertProdAssociateDto
    {
        public long? Id { get; set; }
        public long FK_PId { get; set; }
        public long FK_TCId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
