using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class AddUserToRoleDto
    {
        public long RoleId { get; set; }
        public List<long> Users {  get; set; }
    }
}
