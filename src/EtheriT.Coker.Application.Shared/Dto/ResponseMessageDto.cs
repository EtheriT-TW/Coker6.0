using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto
{
    public class ResponseMessageDto
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
    }
}
