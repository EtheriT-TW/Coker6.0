using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class EditUserDto
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; }
        public string Nickname { get; set; } = "";
        public int? Sex { get; set; }
        public string TelPhone { get; set; } = "";
        public string CellPhone { get; set; } = "";
        public string Email { get; set; }
        public string Address { get; set; } = "";
        public string Account { get; set; } = "";
        public string Birthday { get; set; } = "";
        public long FK_WebsiteId { get; set; } = 0;
        public long FK_RoleId { get; set; } = 0;
    }
}
