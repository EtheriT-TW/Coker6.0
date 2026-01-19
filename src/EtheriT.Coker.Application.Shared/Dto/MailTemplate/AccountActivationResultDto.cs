using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.MailTemplate
{
    public class AccountActivationResultDto
    {
        public string WebsiteName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddDays(1);
    }
}
