using EtheriT.Coker.Application.Webs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Authorizaion.Dto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Account { get; set; }
        public string UserName { get; set; }
        public string token { get; set; }
        public List<WebsDto> Webs { get; set; }
    }
}
