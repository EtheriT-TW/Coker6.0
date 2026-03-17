using EtheriT.Coker.Application.Shared.Dto.enumType.ThirdParty;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class ThirdPartySaveInputDto
    {
        public List<ThirdPartyGroupInputDto>? ThirdParties {  get; set; }
        public List<string>? PaymentType { get; set; }
        public ThirdPartyServiceTypeEnum ServiceType { get; set; }
    }
}
