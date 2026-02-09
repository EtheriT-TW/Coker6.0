using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class DirectoryFacetRange : FullAuditedEntity
    {
        public long FK_DirectoryId { get; set; }
        public int Sort { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public bool Enabled { get; set; }
        public Directory? Directory { get; set; }
    }
}
