
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;

namespace EtheriT.Coker.Application.Shared.Dto.Specification
{
    public class SpecTypeListDto
    {
        public long Id { get; set; }
        public string? Type { get; set; }
        public SeoVariantPropertyEnum? SeoVariantProperty { get; set; }
    }
}
