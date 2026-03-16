using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class LogisticsBox: FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public string Name { get; set; }
        public int CapacityPoint { get; set; }
        public bool IsActive { get; set; }
        public int Sort { get; set; }
        public Website Website { get; set; }
        public ICollection<LogisticsBoxFee> logisticsBoxFees { get; set; }
    }
}
