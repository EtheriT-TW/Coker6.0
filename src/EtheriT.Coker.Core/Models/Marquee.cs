using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class Marquee : FullAuditedEntity
    {
        public long WebsiteId { get; set; }
        [StringLength(60)]
        public string title { get; set; }
        public bool disp_opt { get; set; }
        public int ser_no { get; set; }

        [StringLength(255)]
        public string link { get; set; }
        public bool target { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }

        public Website? Website { get; set; }
    }
}
