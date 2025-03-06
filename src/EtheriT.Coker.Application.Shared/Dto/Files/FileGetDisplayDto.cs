using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class FileGetDisplayDto
    {
        public long Id { get; set; }
        public int FileType { get; set; }
        public string Link { get; set; }
        public string? Name { get; set; }
        public string? Video_Type { get; set; }
    }
}
