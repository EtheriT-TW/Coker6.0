using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class StoreSetDetail : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_SeoSetId { get; set; }
        public string? job_id { get; set; }
        public string? value { get; set; }
        public Website Website { get; set; }
        public SeoSet seoSet { get; set; }
    }
}
