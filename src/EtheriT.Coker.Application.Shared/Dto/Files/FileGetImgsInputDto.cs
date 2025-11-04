using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class FileGetImgsInputDto
    {
        public List<long> Sid { get; set; } = new List<long>();
        public int Type { get; set; }
        public int Size { get; set; }
    }
}
