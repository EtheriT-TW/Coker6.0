using EtheriT.Coker.Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class GetPermissionsOutputDto: ResponseMessageDto
    {
        public List<PermissionsRoleDto> Data {  get; set; } = new List<PermissionsRoleDto>();
    }
}
