using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    /// <summary>
    /// A trusted edge-network range published by a CDN provider.
    /// </summary>
    public sealed class CdnProviderIpRange : FullAuditedEntity
    {
        [StringLength(50)]
        public string Provider { get; set; } = string.Empty;

        [StringLength(64)]
        public string Cidr { get; set; } = string.Empty;

        /// <summary>
        /// IP address version: 4 or 6.
        /// </summary>
        public byte IpVersion { get; set; }

        /// <summary>
        /// Last time this range was confirmed by the provider's official source.
        /// </summary>
        public DateTime LastVerifiedTime { get; set; }
    }
}
