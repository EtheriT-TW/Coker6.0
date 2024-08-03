using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class ThirdPartyGroupInputDto
    {

        public long Id { get; set; }
        public List<ThirdPartyItemInputDto>? value { get; set; }
    }
}
