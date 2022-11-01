using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.Core.Models
{
    [Table("Users")]
    public class User: FullAuditedEntity
    {
        public string Name { get; set; }
        public string? Account { get; set; }
        public string Email { get; set; }
        public string? CellPhone { get; set; }
        public string Password { get; set; }
        public List<MappingUserAndWebsite> Webs { get; set; }
    }
}
