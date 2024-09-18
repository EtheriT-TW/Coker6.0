using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class Advertise_Log: FullAuditedEntity
    {
        public long FK_Adid { get; set; }
        public Guid FK_Tid { get; set; }
        public long? FK_Uid { get; set; }
        public int Action { get; set; }
        public Advertise? Advertise { get; set; }
        public Token? Token { get; set; }
        public User? User { get; set; }
    }
}
