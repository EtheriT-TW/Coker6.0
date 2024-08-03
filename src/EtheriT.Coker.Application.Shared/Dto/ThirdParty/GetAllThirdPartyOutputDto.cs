using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class GetAllThirdPartyOutputDto: ResponseObject
    {
        public List<ThirdPartyItemOutputDto> thirdPartyItems { get; set; } = new List<ThirdPartyItemOutputDto>();

    }
}
