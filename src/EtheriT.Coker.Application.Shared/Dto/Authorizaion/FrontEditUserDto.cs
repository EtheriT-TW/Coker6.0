using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Authorizaion
{
    public class FrontEditUserDto 
    {
        public string? Email { get; set; }
        public string Name { get; set; }
        public int? Sex { get; set; }
        public string? TelPhone { get; set; }
        public string? CellPhone { get; set; }
        public string? Address { get; set; }
        public DateTime? Birthday { get; set; }
    }
}
