using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Shared.JsonObject
{
    public interface IWebsiteCacheStateAppService
    {
        /// <summary>
        /// 取得目前版本，不存在則回傳 0
        /// </summary>
        public Task<long> GetVersionAsync(string cacheKey, string orgName = "");
        /// <summary>
        /// 透過wibsiteId搜尋版本，不存在則回傳 0
        /// </summary>
        public Task<long> GetVersionByWebsiteIdAsync(long websiteId, string cacheKey);
        /// <summary>
        /// 取得目前版本，不存在則建立並回傳 initialVersion
        /// </summary>
        public Task<long> EnsureVersionByWebsiteIdAsync(long websiteId, string cacheKey, long initialVersion = 1);

        /// <summary>
        /// 版本 +1，不存在則建立為 1
        /// </summary>
        public Task<long> TouchAsync(string cacheKey, string orgName = "");
    }
}
