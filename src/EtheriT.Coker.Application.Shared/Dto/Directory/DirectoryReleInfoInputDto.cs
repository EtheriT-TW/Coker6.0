
namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryReleInfoInputDto
    {
        public List<long> Ids { get; set; }
        public long? SiteId { get; set; }
        public int? Page { get; set; }
        public int? ShowNum { get; set; }
        public int? MaxLen { get; set; }
        public int? TotalPage { get; set; }
        public string? SearchText { get; set; }
        public string? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<long>? FilterTagId { get; set; }
    }
}
