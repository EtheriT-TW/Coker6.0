using EtheriT.Coker.Application.Shared.Dto.Order;

namespace EtheriT.Coker.Application.Shared.Order
{
    public interface IRevenueReportAppService
    {
        Task<RevenueReportOutputDto> GetRevenue(
            RevenueReportGranularity granularity,
            DateTime startDate,
            DateTime endDate);
    }
}