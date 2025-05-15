using EtheriT.Coker.Application.Shared.Dto.enumType.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class HeaderTemplateDto: ResponseObject
    {
        public HeadTypeEnum HeadType { get; set; }
        public HeaderContentConfigDto ContentConfig { get; set; } = new HeaderContentConfigDto();
    }
}
