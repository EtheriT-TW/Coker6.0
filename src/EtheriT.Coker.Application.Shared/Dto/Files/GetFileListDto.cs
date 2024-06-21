using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class GetFileListDto
    {
        public long Id { get; set; }
        public GrapesPageTypeEnum type { get; set; }
    }
}
