
namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectorySwitchPageDto
    {
        public long id { get; set; }
        public List<long>? dirids { get; set; }
        public string? filters { get; set; }
        public string? searchtext { get; set; }
        public string? SearchSortBy { get; set; }
        public string? SearchSortDirection { get; set; }
        public string? ProductNameFilter { get; set; }
        public string? Facet { get; set; }
        public long DirectoryType { get; set; }
        public int? MaxLen { get; set; }
        public string? ContentSource { get; set; }
        public string routername { get; set; }
        public int type { get; set; }
    }
}
