using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class SavePermissionsItem
    {
        public string Name { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
	}
}
