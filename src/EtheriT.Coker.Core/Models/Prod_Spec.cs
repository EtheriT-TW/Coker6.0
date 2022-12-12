using EtheriT.Coker.Core.Entity;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_Spec : FullAuditedEntity
    {
        public long FK_Tid { get; set; }
        public string Title { get; set; }
        public Prod_Spec_Type? Prod_Spec_Type { get; set; }
    }
}
