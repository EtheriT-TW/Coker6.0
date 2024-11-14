using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.ThirdParty.LinePayDto
{
    public class LinePayOptionDto
    {
        public LinePayPaymentDto payment { get; set; }
        public LinePayDisplayDto display { get; set; }
        public class LinePayPaymentDto
        {
            public bool capture { get; set; }
        }
        public class LinePayDisplayDto
        {
            public string locale { get; set; }
            public bool checkConfirmUrlBrowser { get; set; }
        }
    }
}
