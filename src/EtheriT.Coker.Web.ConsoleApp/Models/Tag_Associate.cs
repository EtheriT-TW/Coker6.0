using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models
{
    [Table("Tag_Associates")]
    public class Tag_Associate : FullAuditedEntity
    {
        public long FK_TId { get; set; }
        public long FK_AId { get; set; }
        public int Type { get; set; }
        public Tag? Tag { get; set; }
        public Article? Article { get; set; }
    }
}
