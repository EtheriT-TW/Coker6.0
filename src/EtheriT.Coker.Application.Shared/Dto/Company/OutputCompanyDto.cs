using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Company
{
    public class OutputCompanyDto : ResponseMessageDto
    {
        public CompanyDto Company { get; set; }
    }
}
