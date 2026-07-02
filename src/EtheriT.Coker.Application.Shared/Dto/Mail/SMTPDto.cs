using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Mail
{
    public class SMTPDto
    {
        public string Url { get; set; } = "mail.coker.ezsale.tw";
        public int Port { get; set; } = 587;
        public SecureSocketOptions UseSSL { get; set; } = SecureSocketOptions.StartTls;
        public string? UserName { get; set; } = "noreply@coker.ezsale.tw";
        public string? Password { get; set; } = "F4hgCXnUcvuC0hbm";
    }
}
