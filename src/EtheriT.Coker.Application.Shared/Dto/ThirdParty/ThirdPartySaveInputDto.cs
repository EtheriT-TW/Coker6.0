using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class ThirdPartySaveInputDto
    {
        public List<ThirdPartyGroupInputDto>? ThirdParties {  get; set; }
        public List<string>? PaymentType { get; set; }
    }
}
