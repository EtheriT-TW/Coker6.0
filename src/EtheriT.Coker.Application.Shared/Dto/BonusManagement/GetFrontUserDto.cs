using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.BonusManagement
{
    public class GetFrontUserDto
    {
        public long? Id { get; set; }
        public Guid UUID { get; set; }
        public string? Name { get; set; }
        public string? Birthday { get; set; }
    }
}
