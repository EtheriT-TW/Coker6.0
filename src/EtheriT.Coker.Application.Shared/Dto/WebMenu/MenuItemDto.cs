using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto
{
    public class MenuItemDto : PowerOptionDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? text { get { return Title; } }
        public string RouterName { get; set; }
        public int PageType { get; set; }
        public string? Description { get; set; }
        public string? icon { get; set; }
        public string? IconId { get; set; }
        public string? IconUrl { get; set; }
        public string? IconImage { get; set; }
        public bool Visible { get; set; }
        public int SerNO { get; set; }
        public bool PopularVisible { get; set; }
        public long? ImgId { get; set; }
        public string? ImgUrl { get; set; }
        public string? ImgName { get; set; }
        public long? OverImgId { get; set; }
        public string? OverImgUrl { get; set; }
        public string? OverImgName { get; set; }
        public string? LinkUrl { get; set; }
        public bool? Target { get; set; }
        public bool LanBar { get; set; }
        public bool VisibleHeader { get; set; }
        public bool VisibleFooter { get; set; }
        public bool VisibleTitle { get; set; }
        public bool IsFromShelves {  get; set; }
        public long? FK_TopNodeId { get; set; }
        public long? FK_RootNodeId { get; set; }
        public List<MenuItemDto>? Children { get; set; }
    }
}
