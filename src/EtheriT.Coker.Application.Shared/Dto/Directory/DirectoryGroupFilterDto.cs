using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryGroupFilterDto
    {
        public long Id { get; set; }
        public List<long> Tags { get; set; } = new List<long>();
    }
}
