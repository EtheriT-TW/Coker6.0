
namespace EtheriT.Coker.Application.Shared.Dto.Specification
{
    public class SpecTypePickListDto
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public List<SpecSpecPickListDto> Specs { get; set; }
    }
}
