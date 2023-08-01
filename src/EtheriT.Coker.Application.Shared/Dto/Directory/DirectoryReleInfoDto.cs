
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryReleInfoDto
    {
        public long Id { get; set; }
        public DirectoryTypeEnum type { get; set; }
        public string? MainImage { get; set; }
        public string? Link { get; set; }
        public string? NodeDate { get; set; }
        public string? Title { get; set; }

        public string? Description { get; set; }

    }
}
