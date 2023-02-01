using EtheriT.Coker.Core.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class ObjectType : FullAuditedEntity
    {
        [MaxLength(150)]
        public string Title { get; set; }
        public int SerNo { get; set; }
        public List<Html_Content> html_Contents { get; set; }
    }
}
