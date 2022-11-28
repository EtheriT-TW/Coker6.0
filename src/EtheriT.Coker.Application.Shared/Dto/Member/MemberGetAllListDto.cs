using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Member
{
    public class MemberGetAllListDto
    {
        public string Name { get; set; }
        public string? CellPhone { get; set; }
        public string? TelPhone { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int level { get; set; }
        public int Total { get; set; }
        public virtual DateTime CreationTime { get; set; }
    }
}
