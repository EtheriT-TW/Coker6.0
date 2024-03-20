using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Role
{
    public class AddRoleDto
    {
        public long? Id { get; set; }
        public bool IsSuperUser { get; set; } = false;
        public string Name { get; set; } = string.Empty;
    }
}
