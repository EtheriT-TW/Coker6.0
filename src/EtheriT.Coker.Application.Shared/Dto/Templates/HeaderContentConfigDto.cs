using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class HeaderContentConfigDto
    {
        public bool showPagePath { get; set; } = true;
        public bool showMarquee { get; set; } = true;
        public List<SliderDto> sliders { get; set; } = new List<SliderDto>();
    }
}
