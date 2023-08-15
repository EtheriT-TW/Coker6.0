using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models.OldDB
{
    [Table("fence")]
    public class Fence
    {
        public int id { get; set; }
        public int cid { get; set; }
        public int type { get; set; }
        public int? ser_no { get; set; }
        public string? disp_opt { get; set; }
        public int? colNum { get; set; }
        public DateTime? cdate { get; set; }
        public DateTime? edate { get; set; }
    }
}
