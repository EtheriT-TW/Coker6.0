using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class PagePermissionOutputDto: ResponseObject
    {
        public List<PermissionsUserCheckDto> Users { get; set; } = new List<PermissionsUserCheckDto>();
        public List<PermissionsRoleCheckDto> Roles { get; set; } = new List<PermissionsRoleCheckDto>();
    }
}
