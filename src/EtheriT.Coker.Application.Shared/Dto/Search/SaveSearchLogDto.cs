using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Search
{
    public class SaveSearchLogDto
    {
        public string Key { get; set; }
        public long FK_WebsiteId { get; set; }
        public long FK_CustSearchId { get; set; }
    }
}
