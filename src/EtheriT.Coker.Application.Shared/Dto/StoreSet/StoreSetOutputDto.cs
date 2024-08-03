using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.StoreSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Dto.StoreSet
{
    public class StoreSetOutputDto
    {
        public string key { get; set; }
        public string name { get; set; }
        public string? memo { get; set; }
        public string? pattern { get; set; }
        public int? maxlength { get; set; }
        public SeoSetDataTypeEnum type { get; set; }
        public List<StoreSetItemOutputDto> storeSetItemOutputDtos { get; set; } = new List<StoreSetItemOutputDto>();
    }
}
