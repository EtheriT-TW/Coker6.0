
using EtheriT.Coker.Application.Shared.Dto.Files;

namespace EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate
{
    public class TechCertDisplayDto
    {
        public long Id { get; set; }
        public List<FileGetImgDto> Img { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
