using Hangfire.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtheriT.Coker.Application.Filters
{
    public class HangfireDashboardAuthorizationFilter: IDashboardAuthorizationFilter
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public HangfireDashboardAuthorizationFilter(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public bool Authorize(DashboardContext context)
        {
            string path = context.Request.Path;
            // 排除靜態資源請求
            if (path.Contains("/css") || path.Contains("/js") || path.Contains("/fonts"))
            {
                return true;
            }

            using (var scope = _serviceScopeFactory.CreateScope()) {
                var loginUserData = scope.ServiceProvider.GetRequiredService<LoginUserData>();
                var isSystemUser = loginUserData.isSystemUser().GetAwaiter().GetResult();
                // 根據使用者的權限判斷是否能夠訪問 Hangfire 儀表板
                return isSystemUser;
            }
        }
    }
}
