using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class SliderDto
    {
        public string? DesktopImage { get; set; }
        public string? MobileImage { get; set; }
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? Description { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonLinkUrl { get; set; }
        public string? Link { get; set; }
        public bool Enabled { get; set; }
    }
}
