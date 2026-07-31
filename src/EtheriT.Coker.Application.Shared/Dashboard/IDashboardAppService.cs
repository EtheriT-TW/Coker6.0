using EtheriT.Coker.Application.Shared.Dto.Dashboard;

namespace EtheriT.Coker.Application.Shared.Dashboard
{
    public interface IDashboardAppService
    {
        Task<DashboardSystemOverviewDto> GetSystemOverview();
        Task<DashboardTrafficOutputDto> GetTraffic(
            int days = 7,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string granularity = "day");
        Task<DashboardTrafficHeatmapOutputDto> GetTrafficHeatmap(int days = 30);
        Task<List<DashboardPopularPageDto>> GetPopularPages(
            int take = 5,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<DashboardCommerceOverviewDto> GetCommerceOverview();
        Task<DashboardOrderTrendOutputDto> GetOrderTrend(int days = 30);
        Task<DashboardContactsOutputDto> GetContacts(int take = 5);
    }
}
