using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class Advertise_Log : FullAuditedEntity
    {
        public long FK_Adid { get; set; }
        public long? FK_UserId { get; set; }
        public Guid UUID { get; set; }
        public int Action { get; set; }
        public Advertise? Advertise { get; set; }
        //public ICollection<Token> Tokens { get; set; }
    }
}
