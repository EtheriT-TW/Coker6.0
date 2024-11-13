using EtheriT.Coker.Core.Entity;
using System.ComponentModel.DataAnnotations;

namespace EtheriT.Coker.Core.Models
{
    public class FrontUser : FullAuditedEntity
    {
        public Guid UUID { get; set; }
        [StringLength(100)]
        public string? Account { get; set; }
        public string Password { get; set; }
        [StringLength(150)]
		public string Name { get; set; }
        [StringLength(150)]
        public string? Email { get; set; }
        [StringLength(50)]
        public string? CellPhone { get; set; }
        [StringLength(50)]
        public string? TelPhone { get; set; }
        public int Status { get; set; }
        public DateTime? OpenDate { get; set; }
        public int? Level { get; set; }
		[StringLength(250)]
		public string? Address { get; set; }
        public int? Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public int ErrorTimes { get; set; }
        public DateTime? LockTime { get; set; }
        public Guid OpenID { get; set; }
        public DateTime OpenIDSendDate { get; set; }
        public Guid? ForgetID { get; set; }
        public DateTime? ForgeIDSendDate { get; set; }
        public DateTime? PrivacyAgreeTime { get; set; }
        public long? FK_User { get; set; }
        public List<MappingFrontUserAndWebsite> Websites { get; set; }
    }
}
