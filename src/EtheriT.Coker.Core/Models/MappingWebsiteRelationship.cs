using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class MappingWebsiteRelationship : FullAuditedEntity
    {
        public long FatherId { get; set; }
        public long WebsiteId { get; set; }
    }
}
