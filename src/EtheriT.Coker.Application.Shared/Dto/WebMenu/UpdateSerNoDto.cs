using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.WebMenu
{
    public class UpdateSerNoDto
    {
        public long Id { get; set; }
        public long? FK_RootNodeId { get; set; }
        public long? FK_TopNodeId { get; set; }
        public int SerNO { get; set; }
    }
}
