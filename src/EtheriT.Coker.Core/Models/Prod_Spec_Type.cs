using EtheriT.Coker.Core.Entity;

using EtheriT.Coker.Application.Shared.Dto.enumType.Product;

namespace EtheriT.Coker.Core.Models
{
    public class Prod_Spec_Type : FullAuditedEntity
    {
        public long FK_WebsiteId { get; set; }
        public string Type { get; set; }
        public SeoVariantPropertyEnum SeoVariantProperty { get; set; } = SeoVariantPropertyEnum.None;
        public List<Prod_Spec> Prod_Specs { get; set; }
        public Website? Website { get; set; }
    }
}
