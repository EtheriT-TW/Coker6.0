using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
	public class Tag : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(50)] public string Title { get; set; }
        public bool IsTemporary { get; set; }
        public List<Tag_Associate>? Tag_Associates { get; set; }
        public List<Tag_TagGroup>? Tag_TagGroups { get; set; }
        public List<UserTagStatistic>? UserTagStatistics { get; set; }
        public Website? Website { get; set; }
    }
}
