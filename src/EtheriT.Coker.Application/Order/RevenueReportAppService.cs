using EtheriT.Coker.Application.Common;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Application.Shared.Dto.Order;
using EtheriT.Coker.Application.Shared.Order;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace EtheriT.Coker.Application.Order
{
    public sealed class RevenueReportAppService : IRevenueReportAppService
    {
        // 與儀表板「有效訂單金額」同一套定義，兩邊數字才對得起來。
        private static readonly OrderStatusEnum[] RevenueOrderStates =
        {
            OrderStatusEnum.已付款,
            OrderStatusEnum.已出貨,
            OrderStatusEnum.已完成
        };

        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;

        public RevenueReportAppService(CokerDbContext db, LoginUserData loginUserData)
        {
            this.db = db;
            this.loginUserData = loginUserData;
        }

        public async Task<RevenueReportOutputDto> GetRevenue(
            RevenueReportGranularity granularity,
            DateTime startDate,
            DateTime endDate)
        {
            var siteId = await loginUserData.GetWebsiteId();
            var isMonthly = granularity == RevenueReportGranularity.Month;
            var rangeStart = isMonthly
                ? new DateTime(startDate.Year, startDate.Month, 1)
                : startDate.Date;
            var rangeEnd = isMonthly
                ? new DateTime(endDate.Year, endDate.Month, 1).AddMonths(1)
                : endDate.Date.AddDays(1);

            var dailyTotals = await db.Order_Headers
                .AsNoTracking()
                .Where(order =>
                    order.FK_WebsiteId == siteId
                    && !order.IsDeleted
                    && !order.IsTemp
                    && RevenueOrderStates.Contains(order.State)
                    && order.CreationTime >= rangeStart
                    && order.CreationTime < rangeEnd)
                .GroupBy(order => new
                {
                    order.CreationTime.Year,
                    order.CreationTime.Month,
                    order.CreationTime.Day
                })
                .Select(group => new RevenueReportItemDto
                {
                    PeriodStart = new DateTime(group.Key.Year, group.Key.Month, group.Key.Day),
                    OrderCount = group.LongCount(),
                    Amount = group.Sum(order => order.Subtotal + order.Freight)
                })
                .ToListAsync();

            return new RevenueReportOutputDto
            {
                Granularity = granularity,
                StartDate = rangeStart,
                EndDate = rangeEnd.AddDays(-1),
                Items = BuildItems(dailyTotals, rangeStart, rangeEnd, isMonthly)
            };
        }

        // 依日或月彙總，並把沒有訂單的期間補 0，報表才不會跳號。
        private static List<RevenueReportItemDto> BuildItems(
            List<RevenueReportItemDto> dailyTotals,
            DateTime rangeStart,
            DateTime rangeEnd,
            bool isMonthly)
        {
            var totalsByPeriod = dailyTotals
                .GroupBy(item => isMonthly
                    ? new DateTime(item.PeriodStart.Year, item.PeriodStart.Month, 1)
                    : item.PeriodStart)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        OrderCount = group.Sum(item => item.OrderCount),
                        Amount = group.Sum(item => item.Amount)
                    });

            var labelFormat = isMonthly ? "yyyy/MM" : "yyyy/MM/dd";
            var items = new List<RevenueReportItemDto>();
            for (var period = rangeStart;
                 period < rangeEnd;
                 period = isMonthly ? period.AddMonths(1) : period.AddDays(1))
            {
                totalsByPeriod.TryGetValue(period, out var total);
                items.Add(new RevenueReportItemDto
                {
                    PeriodStart = period,
                    PeriodLabel = period.ToString(labelFormat, CultureInfo.InvariantCulture),
                    OrderCount = total?.OrderCount ?? 0,
                    Amount = total?.Amount ?? 0
                });
            }

            return items;
        }
    }
}