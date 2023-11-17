using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class AddMapingUserAndWebsiteDto
    {
        public string? emailOrAccount { get; set; }
        public long? UsetId { get; set; }
        public long RoleId { get; set; }
    }
}
