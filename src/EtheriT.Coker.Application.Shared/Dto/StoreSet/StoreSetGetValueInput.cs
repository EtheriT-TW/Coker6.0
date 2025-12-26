using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.StoreSet
{
    public class StoreSetGetValueInput
    {
        public string? key { get; set; }
        public List<string>? keys { get; set; }

        public long? StoreSetGroupId { get; set; }
        public bool RenderTextareaAsHtml { get; set; } = false;
        public long? SiteId { get; set; }
    }
}
