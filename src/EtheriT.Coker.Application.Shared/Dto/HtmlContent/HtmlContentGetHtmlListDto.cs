using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.HtmlContent
{
    public class HtmlContentGetHtmlListDto: ResponseMessageDto
    {
        public List<HtmlContentDetailDto> Conten {  get; set; } = new List<HtmlContentDetailDto>();
    }
}
