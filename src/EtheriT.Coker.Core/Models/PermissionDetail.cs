using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Models
{
    public class PermissionDetail
    {
        public long Id { get; set; }
        [StringLength(300)]
        public string Name { get; set; }
        public long FK_WebsiteId { get; set; }
        public long? FK_UserId { get; set; }
        public long? FK_RoleId { get; set; }
        public long? FK_TargetId { get; set; }
        public bool IsGranted { get; set; }
        public int Type { get; set; }
        public long CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public User? User { get; set; }
        public Role? Role { get; set; }
        public Website Website { get; set; }
    }
}
