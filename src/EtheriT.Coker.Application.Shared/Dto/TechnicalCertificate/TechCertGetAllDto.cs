using EtheriT.Coker.Application.Shared.Dto.Files;

namespace EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate
{
    public class TechCertGetAllDto
    {
        public long Id { get; set; }
        public long FK_PId { get; set; }
        public long FK_TCId { get; set; }
        public bool IsChecked { get; set; }
        public List<FileGetImgDto>? Img { get; set; }
        public string? Title { get; set; }
    }
}
