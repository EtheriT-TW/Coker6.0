using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
	public class Tag_TagGroup : FullAuditedEntity
    {
        public long FK_TId { get; set; }
        public long FK_TGId { get; set; }
        public Tag? Tag { get; set; }
        public Tag_Group? Tag_Group { get; set; }
    }
}
