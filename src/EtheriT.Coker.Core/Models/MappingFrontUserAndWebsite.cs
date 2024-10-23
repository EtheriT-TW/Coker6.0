using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class MappingFrontUserAndWebsite : FullAuditedEntity
    {
        public long FK_UserId { get; set; }
        public long FK_WebsiteId { get; set; }
        public FrontUser User { get; set; }
        public Website Website { get; set; }
    }
}
