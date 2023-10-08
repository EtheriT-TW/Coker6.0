using EtheriT.Coker.Application.Shared.Dto.enumType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.WebMenu
{
    public class GetFrontContenOutputDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SiteName { get; set; }
        public string Description { get; set; }
        public string LinkUrl { get; set; }
        public string CurrentUrl { get; set; }
        public string Html { get; set; }
        public string Css { get; set; }
        public int LayoutType { get; set; }
        public int? Popular { get; set; }
        public bool VisibleHeader { get; set; }
        public bool VisibleFooter { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public HoldPageNameEnum holdPage { get; set; }
    }
}
