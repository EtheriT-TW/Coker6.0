using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class PermissionsUserCheckDto: PermissionsUserDto
    {
        public bool IsChecked { get; set; }
    }
}
