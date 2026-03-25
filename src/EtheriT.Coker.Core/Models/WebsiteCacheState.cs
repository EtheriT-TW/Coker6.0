using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class WebsiteCacheState: FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }

        /// <summary>
        /// 快取類型，例如 Menu / Directory / Search / Sitemap
        /// </summary>
        [StringLength(250)]  
        public string CacheKey { get; set; } = default!;

        /// <summary>
        /// 版本號，每次異動 +1
        /// </summary>
        public long Version { get; set; }
        public Website Website { get; set; } = default!;
    }
}
