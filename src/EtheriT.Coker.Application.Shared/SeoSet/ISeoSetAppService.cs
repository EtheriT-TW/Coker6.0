using EtheriT.Coker.Application.Shared.Dto.SeoSet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.SeoSet
{
    public interface ISeoSetAppService
    {
        public Task<List<SeoSetOutputDto>> getAll();
        public Task<SeoSetOutputDto?> find(string key);
    }
}
