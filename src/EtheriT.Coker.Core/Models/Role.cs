using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Role : FullAuditedEntity
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public long? FK_WebsiteId{ get; set; }
        public bool IsSuperUser { get; set; }
        public List<Prod_Price> Prod_Prices { get; set; }
        public List<MappingUserAndRole> Users { get; set; }
        public List<Permissions> Permissions { get; set; }
        public List<PermissionDetail> PermissionDetails { get; set; }
    }
}
