using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    [Table("Tokens")]
    public class Token
    {
        public Guid id { get; set; }
        public long? UserID { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public string ip { get; set; }
        public User User { get; set; }
    }
}
