
namespace EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate
{
	public class TechCertSelectedDto
    {
        public long? Id { get; set; }
        public long FK_TCId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
