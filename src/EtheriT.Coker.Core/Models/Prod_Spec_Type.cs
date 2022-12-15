using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_Spec_Type : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public string Type { get; set; }
        public List<Prod_Spec> Prod_Specs { get; set; }
        public Website? Website { get; set; }
    }
}
