
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryReleInfoDto
    {
        public long Id { get; set; }
        public DirectoryTypeEnum type { get; set; }
        public string? MainImage { get; set; }
        public string? Link { get; set; }
        public DateTime? NodeDate { get; set; }
        public int SerNo { get; set; }
        public string? Title { get; set; }

        public string? Description { get; set; }

    }
}
