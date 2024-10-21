using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
	public class MappingFrontUserAndRole : FullAuditedEntity
    {
        public long UserId { get; set; }
        public Guid UUID { get; set; }
        public long RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
