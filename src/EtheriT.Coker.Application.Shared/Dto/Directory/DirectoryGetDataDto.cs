
using EtheriT.Coker.Application.Shared.Dto.Tag;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryGetDataDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Type { get; set; }
        public long? FK_MId { get; set; }
        public bool Visible { get; set; }
        public List<TagGetSelectedDto> TagDatas { get; set; }
    }
}
