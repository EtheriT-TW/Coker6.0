using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class MappingOldNewUUID : FullAuditedEntity
    {
        public Guid UserUUID { get; set; }
        public Guid TempUUID { get; set; }
    }
}
