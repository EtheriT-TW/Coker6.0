using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Search
{
    public class CuseSearchListDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSystem { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string ScopeSummary { get; set; } = string.Empty;
    }
}
