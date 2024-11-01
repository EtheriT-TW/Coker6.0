using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Favorites : FullAuditedEntity
    {
        public long FK_AssocId { get; set; }
        public Guid UUID { get; set; }
        public int Type { get; set; }
    }
}
