using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.JsonObject
{
    public class WebsiteCacheStateDto
    {
        public long FK_WebsiteId { get; set; }
        public string CacheKey { get; set; } = string.Empty;
        public long Version { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
