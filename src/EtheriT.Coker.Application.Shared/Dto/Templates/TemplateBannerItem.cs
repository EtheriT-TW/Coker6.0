using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Templates
{
    public class TemplateBannerItem
    {
        public string? DisktopImage { get; set; }
        public string? PhoneImage { get; set; }
        public string? LinkUrl { get; set; }
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string? Description { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonLinkUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }
}
