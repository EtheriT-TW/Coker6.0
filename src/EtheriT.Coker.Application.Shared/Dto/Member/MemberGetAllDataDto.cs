using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Member
{
    public class MemberGetAllDataDto
    {
        public Guid UUID { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string NickName { get; set; }
        public int? Sex { get; set; }
        public int? Status { get; set; }
        public int? Level { get; set; }
        public string Email { get; set; }
        public string? CellPhone { get; set; }
        public string? TelPhone { get; set; }
        public string? Address { get; set; }
        public long? RoleId { get; set; }

    }
}
