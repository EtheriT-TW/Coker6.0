using EtheriT.Coker.Application.Shared.Dto.Dashboard;

namespace EtheriT.Coker.Application.Shared.Dashboard
{
    public interface IDashboardAppService
    {
        Task<DashboardSystemOverviewDto> GetSystemOverview();
        Task<DashboardTrafficOutputDto> GetTraffic(int days = 7);
        Task<List<DashboardPopularPageDto>> GetPopularPages(int take = 5);
        Task<DashboardContactsOutputDto> GetContacts(int take = 5);
    }
}
