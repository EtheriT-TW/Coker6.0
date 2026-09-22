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
        public string? SourcePath { get; set; }
        public string? ThumbnailPath { get; set; }
        public Guid? ThumbnailGuid { get; set; }
        public string? MediumPath { get; set; }
        public Guid? MediumGuid { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public bool WasResized { get; set; }
    }
}
