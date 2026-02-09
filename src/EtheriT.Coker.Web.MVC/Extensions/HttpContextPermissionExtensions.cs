using EtheriT.Coker.Web.MVC.Startup;
using Microsoft.AspNetCore.Http;

namespace EtheriT.Coker.Web.MVC.Extensions
{
    public static class HttpContextPermissionExtensions
    {
        public static ThePermission GetPermission(this HttpContext ctx)
        {
            if (ctx.Items.TryGetValue(CokerItemKeys.Permission, out var v) && v is ThePermission p)
                return p;

            return ThePermission.DenyAll;
        }
        public static bool CheckHasManySystem(this HttpContext ctx)
        {
            if (ctx.Items.TryGetValue(CokerItemKeys.HasManySystem, out var v) && v is bool p)
                return p;

            return false;
        }
    }
}
