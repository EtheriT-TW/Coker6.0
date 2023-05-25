
namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryReleInfoInputDto
    {
        public List<long> Ids { get; set; }
        public long SiteId { get; set; }
        public int? Page { get; set; }
        public int? ShowNum { get; set; }
        public int? TotalPage { get; set; }
    }
}
