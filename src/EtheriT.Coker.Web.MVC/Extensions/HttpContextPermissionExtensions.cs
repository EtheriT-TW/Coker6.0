using EtheriT.Coker.Web.MVC.Common;
using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Http;

namespace EtheriT.Coker.Web.MVC.Extensions
{
    public static class HttpContextPermissionExtensions
    {
        public static T GetItemOrDefault<T>(this HttpContext ctx, object key, T defaultValue = default!)
        {
            if (ctx?.Items == null) return defaultValue;

            return ctx.Items.TryGetValue(key, out var v) && v is T t
                ? t
                : defaultValue;
        }
        public static bool TryGetItem<T>(this HttpContext ctx, object key, out T value)
        {
            value = default!;
            if (ctx?.Items == null) return false;

            if (ctx.Items.TryGetValue(key, out var v) && v is T t)
            {
                value = t;
                return true;
            }
            return false;
        }
        public static ThePermission GetPermission(this HttpContext ctx)
            => ctx.GetItemOrDefault(CokerContextKeys.Permission, ThePermission.DenyAll);
        public static BonusPermission GetBonusPermission(this HttpContext ctx)
            => ctx.GetItemOrDefault(CokerContextKeys.BonusPermission, BonusPermission.DenyAll);
        public static bool CheckHasManySystem(this HttpContext ctx)
            => ctx.GetItemOrDefault(CokerContextKeys.HasManySystem, false);

    }
}
