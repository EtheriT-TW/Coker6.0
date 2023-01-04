using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.WebMenu
{
    public class PageTypeDto: ResponseMessageDto
    {
        public List<EnumDictionaryDto>? Type { get; set; }
    }
}
