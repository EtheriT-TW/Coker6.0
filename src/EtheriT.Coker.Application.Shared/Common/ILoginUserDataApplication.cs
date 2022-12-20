using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application
{
    public interface ILoginUserDataApplication
    {
        public Task<long> GetWebsiteId();
        public Task<string> GetWebsiteName();
        public string? GetClientIP();
        public string GetAuthorization();
        public Guid GetSecret();
        public Task<bool> CheckedWebSiteId(long id);
    }
}
