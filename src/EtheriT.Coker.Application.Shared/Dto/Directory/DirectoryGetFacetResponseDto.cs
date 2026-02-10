using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryGetFacetResponseDto
    {
        public long DirectoryId { get; set; }
        public List<DirectoryFacetItemDto> Items { get; set; } = new();
    }
}
