using EtheriT.Coker.Application.Shared.JsonObject;
using EtheriT.Coker.Core.Models;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EtheriT.Coker.Application.Shared.Dto.enumType;

namespace EtheriT.Coker.Application.JsonObject
{
    public class WebsiteCacheStateAppService: IWebsiteCacheStateAppService
    {
        private readonly LoginUserData loginUserData;
        private readonly CokerDbContext db;
        public WebsiteCacheStateAppService(CokerDbContext db, LoginUserData loginUserData) {
            this.db = db;
            this.loginUserData = loginUserData;
        }
        public async Task<long> GetVersionByWebsiteIdAsync(long websiteId, string cacheKey)
        {
            cacheKey = WebsiteCacheKeys.Normalize(cacheKey);

            if (!WebsiteCacheKeys.IsValid(cacheKey))
                throw new ArgumentException($"不合法的 cacheKey：{cacheKey}", nameof(cacheKey));

            if (websiteId <= 0)
                throw new ArgumentException("websiteId 不可小於等於 0", nameof(websiteId));

            var cacheState = await db.WebsiteCacheStates
                .FirstOrDefaultAsync(e => e.FK_WebsiteId == websiteId && e.CacheKey == cacheKey);

            return cacheState?.Version ?? 0;
        }
        public async Task<long> GetVersionAsync(string cacheKey, string orgName = "") {
            var websiteId = await loginUserData.GetCommonWebsiteId(orgName);
            return await GetVersionByWebsiteIdAsync(websiteId, cacheKey);
        }
        public async Task<long> EnsureVersionByWebsiteIdAsync(long websiteId, string cacheKey, long initialVersion = 1) {
            var version = await GetVersionByWebsiteIdAsync(websiteId, cacheKey);
            if (version == 0)
            {
                if (initialVersion <= 0) initialVersion = 1;
                var cacheState = new WebsiteCacheState
                {
                    CacheKey = cacheKey,
                    FK_WebsiteId = websiteId,
                    Version = initialVersion
                };
                db.WebsiteCacheStates.Add(cacheState);
                await loginUserData.SaveChanges(cacheState);
                version = cacheState.Version;
            }
            return version;
        }
        public async Task<long> TouchAsync(string cacheKey, string orgName = "") {
            cacheKey = WebsiteCacheKeys.Normalize(cacheKey);
            if (!WebsiteCacheKeys.IsValid(cacheKey))
                throw new ArgumentException($"不合法的 cacheKey：{cacheKey}", nameof(cacheKey));

            var websiteId = await loginUserData.GetCommonWebsiteId(orgName);
            if (websiteId <= 0)
                throw new Exception("找不到可用的網站識別");

            var cacheState = await db.WebsiteCacheStates
                    .FirstOrDefaultAsync(e => e.FK_WebsiteId == websiteId && e.CacheKey == cacheKey);

            if (cacheState == null) {
                cacheState = new WebsiteCacheState { 
                    CacheKey = cacheKey,
                    FK_WebsiteId = websiteId,
                    Version = 1
                };
                db.WebsiteCacheStates.Add(cacheState);
            }
            else
            {
                cacheState.Version++;
            }
            await loginUserData.SaveChanges(cacheState);
            return cacheState.Version;
        }
    }
}
