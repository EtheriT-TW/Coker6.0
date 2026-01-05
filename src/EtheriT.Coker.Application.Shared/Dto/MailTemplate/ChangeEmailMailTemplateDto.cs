using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.MailTemplate
{
    public class ChangeEmailMailTemplateDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime CreationTime { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
