using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class SliderDto
    {
        public string? desktopImage { get; set; }
        public string? mobileImage { get; set; }
        public string? title { get; set; }
        public string? subtitle { get; set; }
        public string? link { get; set; }
        public bool enabled { get; set; }
    }
}
