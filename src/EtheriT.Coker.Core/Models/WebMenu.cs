using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class WebMenu : FullAuditedEntity
    {
        [StringLength(100)]
        public string? Title { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; } = 500;
        public int Popular { get; set; } = 0;
        public bool PopularVisible { get; set; } = true;
        public long? ImgId { get; set; }
        public long? OverImgId { get; set; }
        [StringLength(255)]
        public string? LinkUrl { get; set; }
        public string? Target { get; set; }
        public bool LanBar { get; set; }
        public long? FK_TopNodeId { get; set; }
        public long FK_WebsiteId { get; set; }
        public WebMenu? FK_TopNode { get; set; }
        public List<WebMenu>? FK_ChildNodes { get; set; }
        public Website Website { get; set; }
    }
}
