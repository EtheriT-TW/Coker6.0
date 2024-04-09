using EtheriT.Coker.Application.Shared.Dto.Tag;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryReleInfoGetDto
    {
        public int TotalPage { get; set; }
        public int TotalCount { get; set; }
        public List<DirectoryReleInfoDto> ReleInfos { get; set; }
        public List<DirectorySearchTypeListDto> Filter { get; set; } = new List<DirectorySearchTypeListDto>();
    }
}
