using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
	public class MappingUserAndRole : FullAuditedEntity
    {
        public bool IsFront { get; set; }
        public long UserId { get; set; }
        public long RoleId { get; set; }
        public Role? Role { get; set; }
    }
}
