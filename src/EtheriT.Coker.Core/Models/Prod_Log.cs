
using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_Log : FullAuditedEntity
    {
        public long FK_Pid { get; set; }
        public long? FK_Uid { get; set; }
        public Guid FK_Tid { get; set; }
        public int Action { get; set; }
        public string? Db_Name { get; set; }
        public Prod? Prod { get; set; }
        public User? User { get; set; }
        public ICollection<Token> Tokens { get; set; }
    }
}
