using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_Log : FullAuditedEntity
    {
        public long FK_Pid { get; set; }
        public int Action { get; set; }
        public Guid UUID { get; set; }
        public long? FK_UserId { get; set; }
        public string? Db_Name { get; set; }
        public Prod? Prod { get; set; }
        //public ICollection<Token> Tokens { get; set; }
    }
}
