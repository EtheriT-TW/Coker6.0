
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.Tag;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryReleInfoDto
    {
        public long Id { get; set; }
        public long? FId { get; set; }
        public DirectoryTypeEnum type { get; set; }
        public string? MainImage { get; set; }
        public string? Link { get; set; }
        public DateTime? NodeDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public int SerNo { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Address { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public string? Location { get; set; }
        public string? Dirname { get; set; }
        public string? Description { get; set; }
        public string? OrgName { get; set; }
        public string? PriceDisplayText { get; set; }
        public string? Price { get; set; }
        public string? OriPrice { get; set; }
        public string? Bonus { get; set; }
        public string? SuggestPrice { get; set; }
        public bool IsTimePrice { get; set; }
        public string? ItemNo { get; set; }
        public int? ClickTimes { get; set; }
        public int? ExposureTimes { get; set; }
        public ProdStatusEnum Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool Visible { get; set; }
        public bool Available { get; set; }
        public List<TagGetSelectedDto>? tags { get; set; }

    }
}
