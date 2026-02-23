
using EtheriT.Coker.Application.Shared.Dto.Files;
using EtheriT.Coker.Application.Shared.Dto.Newsletter;
using EtheriT.Coker.Application.Shared.Dto.Tag;

namespace EtheriT.Coker.Application.Shared.Dto.Article
{
	public class ArticleGetDataDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; }
        public bool PopularVisible { get; set; }
        public long FK_WebsiteId { get; set; }
        public DateTime? NodeDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool permanent { get; set; }
        public bool RemovedFromShelves { get; set; }
        public NewsletterFrameDto? DataJson { get; set; }
        public List<TagGetSelectedDto> TagDatas { get; set; }
        public List<FileGetArticleDisplayDto> Files { get; set; }
    }
}
