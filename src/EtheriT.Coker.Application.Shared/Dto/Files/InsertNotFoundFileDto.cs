using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Files
{
    public class InsertNotFoundFileDto
    {
        public long FK_WebsiteID { get; set; }
        public string Url {  get; set; }
        public string From { get; set; }
    }
}
