using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto
{
    public class HtmlContentListOutpotDto: ResponseMessageDto
    {
        public List<HtmlContentDto>? List { get; set; }

    }
}
