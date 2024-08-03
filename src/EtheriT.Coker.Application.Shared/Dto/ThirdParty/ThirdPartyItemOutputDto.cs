using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class ThirdPartyItemOutputDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<ThirdPartyKeypairItemOutputDto> ThirdPartyKeypairs { get; set; } = new List<ThirdPartyKeypairItemOutputDto>();
        public List<PaymentTypeItemOutputDto> PaymentTypes { get; set; } = new List<PaymentTypeItemOutputDto>();
    }
}