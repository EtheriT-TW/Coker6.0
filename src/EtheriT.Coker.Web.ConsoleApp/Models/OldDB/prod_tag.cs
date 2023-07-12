using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models.OldDB
{
    [Table("prod_tag")]
    public class prod_tag
    {
        public int id { get; set; }
        public string? prod_id { get; set; }
        public string? tag_id { get; set; }
        public string? cdate { get; set; }
        public string? edate { get; set; }
        public string? type { get; set; }
        public int? son_tag_id { get; set; }
    }
}
