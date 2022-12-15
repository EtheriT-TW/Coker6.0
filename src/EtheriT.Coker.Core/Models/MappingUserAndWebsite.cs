using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class MappingUserAndWebsite: FullAuditedEntity
    {
        public long UserId { get; set; }
        public long WebsiteId { get; set; }
        public User? User { get; set; }
        public Website? Website { get; set; }
    }
}
