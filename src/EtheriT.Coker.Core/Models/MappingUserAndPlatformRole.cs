using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models;

public sealed class MappingUserAndPlatformRole : FullAuditedEntity
{
    public long UserId { get; set; }
    public long PlatformRoleId { get; set; }
    public User? User { get; set; }
    public PlatformRole? PlatformRole { get; set; }
}
