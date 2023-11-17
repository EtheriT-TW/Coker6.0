using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class PermissionsRoleDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<PermissionsUserDto> Members { get; set; } = new List<PermissionsUserDto>();
    }
}
