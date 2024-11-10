using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Webs
{
    public class FrameCssDto
    {
        public long? Id { get; set; }
        public string Css { get; set; } = string.Empty;
    }
}
