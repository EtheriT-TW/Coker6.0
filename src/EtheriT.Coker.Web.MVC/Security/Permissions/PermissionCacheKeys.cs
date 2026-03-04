using EtheriT.Coker.Web.MVC.Common;
using EtheriT.Coker.Web.MVC.Startup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Web.MVC.Security.Permissions
{
    public static class PermissionCacheKeys
    {
        public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

        private static string GetPrefixOrThrow<T>()
        {
            var t = typeof(T);

            if (t == typeof(ThePermission))
                return CokerContextKeys.Permission;

            if (t == typeof(BonusPermission))
                return CokerContextKeys.BonusPermission;

            throw new InvalidOperationException(
                $"Unsupported permission type: {t.FullName}");
        }

        public static string For<T>(long websiteId, long userId)
        {
            var prefix = GetPrefixOrThrow<T>();
            return $"{prefix}:{websiteId}:{userId}";
        }
    }
}
