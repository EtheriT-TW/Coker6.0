
namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class FileDataChangeDto
    {
        public long Id { get; set; }
        public long SId { get; set; }
        public int? SerNo { get; set; }
        public bool? IsVisible { get; set; }
        public string? AreaKey { get; set; }
    }
}
