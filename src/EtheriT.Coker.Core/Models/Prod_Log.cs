using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_Log
    {
        public long Id { get; set; }
        public long FK_Pid { get; set; }
        public LogActionEnum Action { get; set; }
        public Guid UUID { get; set; }
        public long? FK_UserId { get; set; }
        [MaxLength(300)]
        public string Remark { get; set; } = string.Empty;
        public virtual DateTime CreationTime { get; set; } = DateTime.Now;
        public Prod? Prod { get; set; }
    }
}
