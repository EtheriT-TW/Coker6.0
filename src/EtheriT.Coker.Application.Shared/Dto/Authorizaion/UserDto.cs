using EtheriT.Coker.Application.Shared.Dto.Authorizaion;
using EtheriT.Coker.Application.Webs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Authorizaion.Dto
{
    public class UserDto: UserSimplifyDto
	{
        public string ProfileImage { get; set; } = string.Empty;
        public List<WebsDto>? Webs { get; set; }
    }
}
