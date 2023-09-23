using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class StoreSetDetail : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public long FK_StoreSetId { get; set; }
        public bool enable { get; set; }
        public string? value { get; set; }
        public Website Website { get; set; }
        public StoreSet StoreSet { get; set; }
    }
}
