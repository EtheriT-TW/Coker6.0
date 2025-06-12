using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class ImageConversionResultDto
    {
        public string Path { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public long FileLength { get; set; } = 0;
    }
}
