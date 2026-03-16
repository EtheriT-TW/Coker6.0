using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty
{
    public class ThirdPartyKeypairItemOutputDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string PromptText { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
    }
}
