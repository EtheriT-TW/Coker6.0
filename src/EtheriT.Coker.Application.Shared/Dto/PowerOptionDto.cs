using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto
{
    public class PowerOptionDto
    {
        public bool? CanEdit { get; set; } = true;
        public bool? CanDel { get; set; } = true;
        public bool? CanAdd { get; set; } = true;
        public bool? CanView { get; set; } = true;
        public int MaxLevel { get; set; } = -1;
        public int MinLevel { get; set; } = 0;
    }
}
