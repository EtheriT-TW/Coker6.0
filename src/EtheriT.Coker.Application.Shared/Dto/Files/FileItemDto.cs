using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.Files
{
    public class FileItemDto
    {
        public long? Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string? Path { get; set; }
    }
}
