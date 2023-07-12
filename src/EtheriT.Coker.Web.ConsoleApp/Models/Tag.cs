using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.ConsoleApp.Models
{
    [Table("Tags")]
    public class Tag : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        [StringLength(50)] public string Title { get; set; }
        public List<Tag_Associate>? Associates { get; set; }
    }
}
