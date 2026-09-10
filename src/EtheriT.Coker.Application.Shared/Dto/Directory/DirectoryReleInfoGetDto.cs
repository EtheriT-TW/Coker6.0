using EtheriT.Coker.Application.Shared.Dto.Tag;

using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryReleInfoGetDto
    {
        public List<long>? Ids { get; set; }
        public int TotalPage { get; set; }
        public int TotalCount { get; set; }
        public DirectoryTypeEnum? ContentType { get; set; }
        public List<DirectoryReleInfoDto> ReleInfos { get; set; }
        public List<DirectoryListBySearchDto> DirectoryType {  get; set; } = new List<DirectoryListBySearchDto>();
        public List<DirectorySearchTypeListDto> Filter { get; set; } = new List<DirectorySearchTypeListDto>();
    }
}
