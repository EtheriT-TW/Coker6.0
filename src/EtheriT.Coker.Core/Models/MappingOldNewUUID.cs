using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class MappingOldNewUUID : FullAuditedEntity
    {
        public Guid OldUUID { get; set; }
        public Guid NewUUID { get; set; }
    }
}
