using EtheriT.Coker.Application.Dto.StoreSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.Dto.StoreSet
{
    public class StoreSetDetailOutputDto
    {
        public string key { get; set; }
        public List<string>? value { get; set; }
    }
}
