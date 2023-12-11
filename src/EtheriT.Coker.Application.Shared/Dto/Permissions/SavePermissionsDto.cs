using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class SavePermissionsDto
    {
        public long? FK_UserId { get; set; }
        public long? FK_RoleId { get; set; }
        public long? SiteId { get; set; }
        public List<SavePermissionsItem> Items {  get; set; } = new List<SavePermissionsItem>();
    }
}
