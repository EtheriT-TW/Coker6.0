
namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryGetListDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; }
        public long? FK_Mid { get; set; }
        public string Items { get; set; }
        public bool Visible { get; set; }
    }
}
