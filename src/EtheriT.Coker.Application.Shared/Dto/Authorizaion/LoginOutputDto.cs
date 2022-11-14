using EtheriT.Coker.Application.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtheriT.Coker.Application.Dto;

namespace EtheriT.Coker.Application.Authorizaion.Dto
{
    public class LoginOutputDto: ResponseMessageDto
    {
        public string? Token { get; set; }
        public Guid? Secret { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
