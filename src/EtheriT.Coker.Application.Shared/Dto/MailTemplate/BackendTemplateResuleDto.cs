using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.MailTemplate
{
    public class BackendTemplateResuleDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public string SetPasswordUrl { get; set; } = string.Empty;
        public DateTime ExpireTime { get; set; } = DateTime.Now;
    }
}
