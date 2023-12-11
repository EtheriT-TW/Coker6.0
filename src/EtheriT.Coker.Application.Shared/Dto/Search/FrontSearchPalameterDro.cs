using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Search
{
    public class FrontSearchPalameterDro
    {
        public List<SearchItemDto> Class { get; set; }
        public long SearchId { get; set; }
        public string SearchText { get; set; }
    }
}
