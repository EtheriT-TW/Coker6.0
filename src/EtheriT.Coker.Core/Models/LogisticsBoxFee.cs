using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class LogisticsBoxFee: FullAuditedEntity
    {
        public long FK_LogisticsSettingId { get; set; }
        public long FK_LogisticsBoxId { get; set; }
        public decimal Fee { get; set; }
        public LogisticsSetting LogisticsSetting { get; set; }
        public LogisticsBox logisticsBox { get; set; }
    }
}
