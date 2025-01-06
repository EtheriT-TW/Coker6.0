using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.Search
{
    public class SearchKeyListDto: ResponseObject
    { 
        public List<SearchKeyDto> Keys { get; set; } = new List<SearchKeyDto>();
        public DateTime LastInsertTime {  get; set; }
    }
    public class SearchKeyDto
    {
        public string Key { get; set; } = string.Empty;
        public int Times { get; set; }
    }
}
