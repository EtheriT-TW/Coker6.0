using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.HtmlContent
{
    public class HtmlContentDetailDto
    {
        public int Id { get; set; }
        public string? Html { get; set; }
        public string? Css { get; set; }
    }
}
