using EtheriT.Coker.Application.Shared.Dto.enumType.WebsiteCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.JsonObject
{
    public class JsonObjectAddDto
    {
        public string CacheKey { get; set; } = default!;
        public long CacheVersion { get; set; }
        public long? FK_WebsiteId { get; set; }
        public long? FK_AId { get; set; }
        public string Json {  get; set; } = default!;
    }
}
