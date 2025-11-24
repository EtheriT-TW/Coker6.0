using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectoryFilterDto
    {
        public DirectorySearchTypeEnum Type { get; set; }
        public List<DirectoryGroupFilterDto> Group { get; set; } = new List<DirectoryGroupFilterDto>();
    }
}
