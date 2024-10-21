using EtheriT.Coker.Core.Entity;
using EtheriT.Coker.Web.Core.Models;

namespace EtheriT.Coker.Core.Models
{
    public class MappingFrontUserAndWebsite : FullAuditedEntity
    {
        public long FK_UserId { get; set; }
        public long FK_WebsiteId { get; set; }
        public Guid UUID { get; set; }
        public int Status {  get; set; }
        public DateTime? OpenDate { get; set; }
        public Guid OpenID { get; set; }
        public DateTime OpenIDSendDate { get; set; }
        public Guid? ForgetID { get; set; }
        public DateTime? ForgeIDSendDate { get; set; }
        public FrontUser? User { get; set; }
        public Website? Website { get; set; }
        public List<Prod_Log> Prod_Logs { get; set; }
        public List<Advertise_Log> Advertise_Logs { get; set; }
        public List<Remote> Remotes { get; set; }
    }
}
