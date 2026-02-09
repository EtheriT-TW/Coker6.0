using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryFacetRangeDto
    {
        public long? DirectoryId { get; set; }
        public int Sort { get; set; } 
        public int Start { get; set; } 
        public int End { get; set; }
        public bool Enabled { get; set; } = true;
    }

}
