
namespace EtheriT.Coker.Application.Shared.Dto.TechnicalCertificate
{
    public class TechnicalCertificateDto
    {
        public long Id { get; set; }
        public Guid TId { get; set; }
        public bool Disp_opt { get; set; }
        public string? Img { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Ser_no { get; set; }
        public virtual DateTime? StartDate { get; set; }
        public virtual DateTime? EndDate { get; set; }
        public bool Permanent { get; set; }
    }
}
