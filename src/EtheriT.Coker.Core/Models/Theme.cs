using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Theme: FullAuditedEntity
    {
        public long FK_WebsiteID {  get; set; }
        public int LayoutType { get; set; }
        public int HeadType { get; set; }
        public int FoodType { get; set; }
        public string Css { get; set; } = string.Empty;
        public bool Enable { get; set; }
        public Website Website { get; set; }
    }
}
