using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Mail
{
    public class SMTPDto
    {
        public string Url { get; set; } =  "msa.hinet.net";
        public int Port { get; set; } = 25;
        public bool useSSL { get; set; } = true;
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
