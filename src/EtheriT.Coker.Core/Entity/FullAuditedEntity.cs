using EtheriT.Coker.Web.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Core.Entity
{
    public abstract class FullAuditedEntity: AuditedEntity
    {
        public virtual long? DeleterUserId { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual DateTime? DeletionTime { get; set; }
    }
}
