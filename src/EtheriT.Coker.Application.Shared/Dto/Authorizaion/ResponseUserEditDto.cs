using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class ResponseUserEditDto: ResponseMessageDto
    {
        public EditUserDto data { get; set; } = new EditUserDto();
    }
}
