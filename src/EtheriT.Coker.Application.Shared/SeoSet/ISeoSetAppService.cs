using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.SeoSet
{
    public interface ISeoSetAppService
    {
        public void getAll();
        public void find(string key);
    }
}
