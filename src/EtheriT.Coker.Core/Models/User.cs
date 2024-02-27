using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.Core.Models
{
    [Table("Users")]
    public class User : FullAuditedEntity
    {
		[StringLength(150)]
		public string Name { get; set; }
		[StringLength(50)]
		public string? Nickname { get; set; }
		public int? Sex { get; set; }
        public int? Status { get; set; }
        public int? Level { get; set; }
		[StringLength(100)]
		public string? Account { get; set; }
		[StringLength(150)]
		public string Email { get; set; }
		[StringLength(50)]
		public string? CellPhone { get; set; }
		[StringLength(50)]
		public string? TelPhone { get; set; }
		[StringLength(250)]
		public string? Address { get; set; }
        public int? Total { get; set; }
        public string? UniformId { get; set; }
        public string Password { get; set; }
        public int ErrorTimes { get; set; }
        public DateTime? LockTime { get; set; }
        public List<MappingUserAndWebsite> Webs { get; set; }
        public List<MappingUserAndRole> Roles { get; set; }
        public List<Prod_Log> Prod_Logs { get; set; }
		public List<Permissions> Permissions { get; set; }
		public List<Remote> Remotes { get; set; } = new List<Remote>();
	}
}
