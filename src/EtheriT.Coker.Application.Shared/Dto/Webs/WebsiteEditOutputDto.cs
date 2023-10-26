using EtheriT.Coker.Application.Dto;
using EtheriT.Coker.Application.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Webs
{
    public class WebsiteEditOutputDto: ResponseMessageDto
    {
        public WebsiteEditDto Website {  get; set; }
        public CompanyDto Company { get; set; }
    }
}
