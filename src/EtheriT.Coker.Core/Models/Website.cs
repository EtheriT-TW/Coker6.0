using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Website: FullAuditedEntity
    {
        public string Title { get; set; }
        public string? DefaultUrl { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public string Locale { get; set; }
        public string Type { get; set; }
        public string? Keywords { get; set; }
        public List<MappingUserAndWebsite> Users { get; set; }
        public List<Marquee> Marquees { get; set; }
        public List<WebMenu> WebMenus { get; set; }
    }
}
