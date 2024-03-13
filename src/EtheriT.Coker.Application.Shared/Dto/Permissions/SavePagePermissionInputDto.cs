using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class SavePagePermissionInputDto: GetPagePermissionInputDto
    {
        public List<long> Users { get; set; } = new List<long>();
        public List<long> Roles { get; set;} = new List<long>();
    }
}
