using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.MailTemplate
{
    public class ForgetTemplateResultDto
    {
        public string Email { get; set; }
        public string WebsiteLink { get; set; }
        public Guid ForgetID { get; set; }
        public DateTime ForgeIDSendDate { get; set; }
    }
}
