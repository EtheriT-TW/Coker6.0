using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Permissions
{
    public class GetPagePermissionInputDto
    {
        public bool isFront { get; set; }
        public long PageId { get; set; }
        public PermissionDetailsTypeEnum Type {  get; set; }  
    }
}
