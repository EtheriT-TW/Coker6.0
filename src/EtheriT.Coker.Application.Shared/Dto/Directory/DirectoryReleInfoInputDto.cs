
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
        public string? Target { get; set; }
        public long DirectoryType { get; set; } = 0;
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public bool? FindNearest { get; set; } = false;
        public string? Facet { get; set; } = string.Empty;
        public List<DirectoryFilterDto> Filters { get; set; } = new List<DirectoryFilterDto>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<long>? FilterTagId { get; set; }
    }
}
