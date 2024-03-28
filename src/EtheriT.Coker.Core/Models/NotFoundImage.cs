using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class NotFoundImage
    {
        public long Id { get; set; }
        [StringLength(255)]
        public string Url { get; set; }
        [StringLength(255)]
        public string From { get; set; }
        public DateTime CreateDate { get; set; }
        public long FK_WebsiteId { get; set; }
        public Website Website { get; set; }
    }
}
