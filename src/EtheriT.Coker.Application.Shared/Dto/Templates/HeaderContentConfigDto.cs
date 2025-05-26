using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class HeaderContentConfigDto
    {
        public bool ShowPagePath { get; set; } = true;
        public bool ShowMarquee { get; set; } = true;
        public List<SliderDto> Sliders { get; set; } = new List<SliderDto>();
    }
}
