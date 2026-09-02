using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// Tracks CDN range synchronization and administrator-alert state.
    /// </summary>
    public sealed class CdnProviderSyncState : FullAuditedEntity
    {
        [StringLength(50)]
        public string Provider { get; set; } = string.Empty;

        public DateTime? LastAttemptTime { get; set; }
        public DateTime? LastSuccessTime { get; set; }
        public int ConsecutiveFailureCount { get; set; }

        [StringLength(4000)]
        public string? LastError { get; set; }

        public bool AlertSent { get; set; }
        public DateTime? LastAlertTime { get; set; }
    }
}
