using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.MailTemplate
{
    public class MailTemplateInputDto
    {
        public string? Key { get; set; }
        public required object Model { get; set; }
    }
}
