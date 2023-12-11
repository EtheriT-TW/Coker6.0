using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Mail
{
    public class SenderDto
    {
        public MailUserDataDto Sender { get; set; } = new MailUserDataDto { Email= "servise@ether.com.tw" };
        public List<MailUserDataDto> Recipients { get; set; } = new List<MailUserDataDto>();
        public List<MailUserDataDto> Bcc { get; set; } = new List<MailUserDataDto>();
        public List<MailUserDataDto> CC { get; set; } = new List<MailUserDataDto>();
        public List<string> FilePath { get; set; } = new List<string>();
        public string Subject { get; set; } = string.Empty;
        public string TextBody { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Css { get; set; } = string.Empty;
        public SMTPDto SMTP { get; set; } = new SMTPDto();
    }
}
