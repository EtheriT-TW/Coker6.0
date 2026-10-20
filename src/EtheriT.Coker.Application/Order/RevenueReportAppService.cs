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
        private static readonly OrderStatusEnum[] RevenueOrderStates =
        {
            OrderStatusEnum.已付款,
            OrderStatusEnum.已出貨,
            OrderStatusEnum.已完成
        };

        private static readonly OrderStatusEnum[] RevenueInvalidOrderStatus =
        {
            OrderStatusEnum.付款失敗,
            OrderStatusEnum.已取消
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
                    && (RevenueOrderStates.Contains(order.State) || RevenueInvalidOrderStatus.Contains(order.State))
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
                    OrderCount = group.LongCount(order => RevenueOrderStates.Contains(order.State)),
                    Amount = group.Sum(order => RevenueOrderStates.Contains(order.State) ? order.Subtotal + order.Freight : 0),
                    InvalidOrderCount = group.LongCount(order => RevenueInvalidOrderStatus.Contains(order.State)),
                    InvalidAmount = group.Sum(order => RevenueInvalidOrderStatus.Contains(order.State) ? order.Subtotal + order.Freight : 0)

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
                        Amount = group.Sum(item => item.Amount),
                        InvalidOrderCount = group.Sum(item => item.InvalidOrderCount),
                        InvalidAmount = group.Sum(item => item.InvalidAmount)
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
                    Amount = total?.Amount ?? 0,
                    InvalidOrderCount = total?.InvalidOrderCount ?? 0,
                    InvalidAmount = total?.InvalidAmount ?? 0,
                    AverageOrderValue = CalculateAverageOrderValue(total?.Amount ?? 0, total?.OrderCount ?? 0)
                });
            }

            return items;
        }

        private static decimal CalculateAverageOrderValue(decimal amount, long orderCount)
        {
            if (orderCount == 0)
                return 0;

            return Math.Round(amount / orderCount, 0, MidpointRounding.AwayFromZero);
        }
    }
}