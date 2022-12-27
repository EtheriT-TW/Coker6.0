
namespace EtheriT.Coker.Application.Shared.Dto.Product
{
	public class ProductTechCertDto
    {
        public long Id { get; set; }
        public long FK_PId { get; set; }
        public long FK_TCId { get; set; }
        public bool IsChecked { get; set; }
    }
}
