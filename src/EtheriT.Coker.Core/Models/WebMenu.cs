using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class WebMenu : FullAuditedEntity
    {
        [StringLength(100)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [StringLength(50)]
        public string? icon { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; } = 500;
        public int Popular { get; set; } = 0;
        public bool PopularVisible { get; set; } = true;
        public long? ImgId { get; set; }
        public long? OverImgId { get; set; }
        [StringLength(255)]
        public string? LinkUrl { get; set; }
        public bool? Target { get; set; }
        public bool LanBar { get; set; }
        [MaxLength]
        public string? SaveHtml { get; set; }
        [MaxLength]
        public string? SaveCss { get; set; }
        [MaxLength]
        public string? Html { get; set; }
        [MaxLength]
        public string? Css { get; set; }
        public int PageType { get; set; }
        [StringLength(50)]
        public string RouterName { get; set; }
        public long? FK_TopNodeId { get; set; }
        public long? FK_RootNodeId { get; set; }
        public long FK_WebsiteId { get; set; }
        public bool VisibleHeader { get; set; }
        public bool VisibleFooter { get; set; }
        public bool VisibleTitle { get; set; }
        public WebMenu? FK_TopNode { get; set; }
        public WebMenu? FK_RootNode { get; set; }
        public List<WebMenu>? FK_ChildNodes { get; set; }
        public Website Website { get; set; }
    }
}
