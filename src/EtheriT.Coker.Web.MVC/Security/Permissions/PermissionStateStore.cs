using EtheriT.Coker.Web.MVC.Common;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.WebRequestMethods;

namespace EtheriT.Coker.Web.MVC.Security.Permissions
{
    public class PermissionStateStore
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _ctx;

        public PermissionStateStore(IMemoryCache cache, IHttpContextAccessor ctx)
        {
            _cache = cache;
            _ctx = ctx;
        }
        public void Set<T>(long websiteId, long userId, T value)
        {
            var ctx = _ctx.HttpContext
                      ?? throw new InvalidOperationException("No active HttpContext.");

            var prefix = PermissionCacheKeys
                        .For<T>(websiteId, userId)
                        .Split(':')[0];

            ctx.Items[prefix] = value!;
            _cache.Set(
                PermissionCacheKeys.For<T>(websiteId, userId),
                value!,
                PermissionCacheKeys.Ttl
            );
        }
        public bool TryGet<T>(long websiteId, long userId, out T value)
        {
            return _cache.TryGetValue(
                        PermissionCacheKeys.For<T>(websiteId, userId),
                        out value!
                    );
        }

        // 你若想保留 DenyAll 的 fallback，可提供這個 convenience method
        public T GetOrDefault<T>(long websiteId, long userId, T defaultValue)
            => TryGet<T>(websiteId, userId, out var v) ? v : defaultValue;
    }
}
