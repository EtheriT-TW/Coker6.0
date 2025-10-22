using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class MappingLogisticsSettingAndProd
    {
        public long FK_LogisticsSettingId { get; set; }
        public long FK_ProdId { get; set; }
        public LogisticsSetting? LogisticsSetting { get; set; }
        public Prod? Prod { get; set; }
    }
}
