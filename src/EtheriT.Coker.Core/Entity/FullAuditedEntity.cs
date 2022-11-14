using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Entity
{
    public abstract class FullAuditedEntity
    {
        public virtual long Id { get; set; }
        public virtual long CreatorUserId { get; set; }
        public virtual long? DeleterUserId { get; set; }
        public virtual long? LastModifierUserId { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual DateTime CreationTime { get; set; } = DateTime.Now;
        public virtual DateTime? DeletionTime { get; set; }
        public virtual DateTime? LastModificationTime { get; set; }
    }
}
