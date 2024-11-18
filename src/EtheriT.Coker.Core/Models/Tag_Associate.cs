using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
	public class Tag_Associate : FullAuditedEntity
    {
        public long FK_TId { get; set; }
        public long FK_AId { get; set; }
        public TagAssociateTypeEnum Type { get; set; }
        public Tag? Tag { get; set; }
    }
}
