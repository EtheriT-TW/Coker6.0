using EtheriT.Coker.Application.Shared.Dto.enumType.Directory;
using EtheriT.Coker.Application.Shared.Dto.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Directory
{
    public class DirectorySearchTypeListDto
    {
        public long? Id {  get; set; }
        public DirectorySearchTypeEnum Type {  get; set; }
        public string Name { get; set; }
        public List<TagGetSelectedDto>? Tags { get; set; }
    }
}
