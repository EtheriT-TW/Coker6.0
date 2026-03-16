
namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class GetAllThirdPartyOutputDto: ResponseObject
    {
        public List<ThirdPartyItemOutputDto> thirdPartyItems { get; set; } = new List<ThirdPartyItemOutputDto>();

    }
}
