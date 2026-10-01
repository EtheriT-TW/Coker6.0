using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models;

public sealed class PlatformAdministratorInvitation : FullAuditedEntity
{
    public long UserId { get; set; }
    public long MvcRoleId { get; set; }
    public long PlatformRoleId { get; set; }
    public string InvitedEmail { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? EmailVerifiedAtUtc { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public long? ApprovedByUserId { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public long? RevokedByUserId { get; set; }

    public User? User { get; set; }
    public Role? MvcRole { get; set; }
    public PlatformRole? PlatformRole { get; set; }
}
