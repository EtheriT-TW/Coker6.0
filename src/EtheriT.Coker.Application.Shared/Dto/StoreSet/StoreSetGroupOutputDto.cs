using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.StoreSet
{
    public class StoreSetGroupOutputDto
    {
        public string Title {get; set;}
        public string Description { get; set;}
        public string Image { get; set;}
        public List<StoreSetOutputDto> storeSets { get; set;}
    }
}
