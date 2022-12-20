using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto
{
    public class MenuItemDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? icon { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; }
        public bool PopularVisible { get; set; }
        public long? ImgId { get; set; }
        public long? OverImgId { get; set; }
        public string? LinkUrl { get; set; }
        public bool? Target { get; set; }
        public bool LanBar { get; set; }
        public long? FK_TopNodeId { get; set; }
        public long? FK_RootNodeId { get; set; }
        public List<MenuItemDto>? Childs { get; set;}
    }
}
