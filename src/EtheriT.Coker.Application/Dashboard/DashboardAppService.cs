using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Dashboard;
using EtheriT.Coker.Application.Shared.Dto.Dashboard;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Order;
using EtheriT.Coker.Application.Shared.Dto.enumType.Product;
using EtheriT.Coker.Application.Shared.FlowSize;
using EtheriT.Coker.Application.BackgroundJob;
using EtheriT.Coker.Application.Common;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EtheriT.Coker.Application.Dashboard
{
    public sealed class DashboardAppService : IDashboardAppService
    {
        private readonly CokerDbContext db;
        private readonly LoginUserData loginUserData;
        private readonly IFlowSizeAppService flowSizeAppService;
        private readonly IUploadPathResolver uploadPathResolver;
        private readonly StringHandler stringHandler;

        public DashboardAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IFlowSizeAppService flowSizeAppService,
            IUploadPathResolver uploadPathResolver,
            StringHandler stringHandler)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.flowSizeAppService = flowSizeAppService;
            this.uploadPathResolver = uploadPathResolver;
            this.stringHandler = stringHandler;
        }

        public async Task<DashboardSystemOverviewDto> GetSystemOverview()
        {
            var orgName = await loginUserData.GetWebsiteOrgName();
            var directoryPath = string.Empty;

            try
            {
                directoryPath = uploadPathResolver.GetRootPath(orgName);
            }
            catch
            {
                // 未設定上傳目錄時，由下方顯示方法回傳提示。
            }

            var today = DateTime.Today;
            return new DashboardSystemOverviewDto
            {
                StorageSize = CalculateDirectorySize(directoryPath),
                StorageUpdatedAt = GetLastChangeDate(directoryPath),
                MonthFlowSize = FormatSize((await flowSizeAppService.GetMonthFlowSizes()).Total),
                MonthRange = $"{today:yyyy-MM}-01 至 {today:yyyy-MM-dd}"
            };
        }

        public async Task<DashboardTrafficOutputDto> GetTraffic(
            int days = 7,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string granularity = "day")
        {
            days = Math.Clamp(days, 1, 30);
            var siteId = await loginUserData.GetWebsiteId();
            var today = DateTime.Today;

            if (string.Equals(granularity, "hour", StringComparison.OrdinalIgnoreCase))
            {
                if (startDate.HasValue || endDate.HasValue || days != 1)
                    throw new ArgumentException("小時趨勢僅支援今日資料。", nameof(granularity));

                return await GetHourlyTraffic(siteId, today);
            }

            var rangeEnd = endDate?.Date ?? today;
            var rangeStart = startDate?.Date ?? rangeEnd.AddDays(-(days - 1));
            var rangeDays = (rangeEnd - rangeStart).Days + 1;

            if (rangeDays < 1 || rangeDays > 366)
                throw new ArgumentOutOfRangeException(nameof(startDate), "查詢區間必須介於 1 至 366 天。");

            var statistics = await db.RemoteDailyStatistics
                .AsNoTracking()
                .Where(statistic =>
                    statistic.FK_WebsiteId == siteId
                    && statistic.Scope == 0
                    && statistic.StatisticDate >= rangeStart
                    && statistic.StatisticDate <= rangeEnd)
                .Select(statistic => new
                {
                    statistic.StatisticDate,
                    statistic.EffectiveViews,
                    statistic.EffectiveUniqueVisitors,
                    statistic.AggregatedAt
                })
                .ToListAsync();

            var statisticsByDate = statistics.ToDictionary(item => item.StatisticDate.Date);
            var items = Enumerable.Range(0, rangeDays)
                .Select(offset =>
                {
                    var date = rangeStart.AddDays(offset);
                    statisticsByDate.TryGetValue(date, out var statistic);
                    return new DashboardTrafficItemDto
                    {
                        Date = date,
                        PageViews = statistic?.EffectiveViews ?? 0,
                        Visitors = statistic?.EffectiveUniqueVisitors ?? 0
                    };
                })
                .ToList();

            return new DashboardTrafficOutputDto
            {
                Items = items,
                Granularity = "day",
                UpdatedAt = statistics.Count == 0
                    ? DateTime.Now
                    : statistics.Max(item => item.AggregatedAt)
            };
        }

        private async Task<DashboardTrafficOutputDto> GetHourlyTraffic(
            long siteId,
            DateTime statisticDate)
        {
            var rangeStart = statisticDate.Date;
            var rangeEnd = rangeStart.AddDays(1);
            var statistics = await db.RemoteHourlyStatistics
                .AsNoTracking()
                .Where(statistic =>
                    statistic.FK_WebsiteId == siteId
                    && statistic.StatisticHour >= rangeStart
                    && statistic.StatisticHour < rangeEnd)
                .Select(statistic => new
                {
                    statistic.StatisticHour,
                    statistic.EffectiveViews,
                    statistic.EffectiveUniqueVisitors,
                    statistic.AggregatedAt
                })
                .ToListAsync();

            var statisticsByHour = statistics.ToDictionary(
                item => item.StatisticHour.Hour);

            return new DashboardTrafficOutputDto
            {
                Granularity = "hour",
                Items = Enumerable.Range(0, 24)
                    .Select(hour =>
                    {
                        statisticsByHour.TryGetValue(hour, out var statistic);
                        return new DashboardTrafficItemDto
                        {
                            Date = statisticDate.AddHours(hour),
                            PageViews = statistic?.EffectiveViews ?? 0,
                            Visitors = statistic?.EffectiveUniqueVisitors ?? 0
                        };
                    })
                    .ToList(),
                UpdatedAt = statistics.Count == 0
                    ? DateTime.Now
                    : statistics.Max(item => item.AggregatedAt)
            };
        }

        public async Task<DashboardTrafficHeatmapOutputDto> GetTrafficHeatmap(int days = 30)
        {
            if (days != 7 && days != 30)
                throw new ArgumentOutOfRangeException(nameof(days), "時段分析僅支援近 7 天或近 30 天。");

            var siteId = await loginUserData.GetWebsiteId();
            var rangeEnd = DateTime.Today.AddDays(-1);
            var rangeStart = rangeEnd.AddDays(-(days - 1));

            var completedDates = (await db.RemoteDailyAggregationRuns
                    .AsNoTracking()
                    .Where(run =>
                        run.AggregationVersion == RemoteDailyStatisticsWorking.CurrentAggregationVersion
                        && run.StatisticDate >= rangeStart
                        && run.StatisticDate <= rangeEnd)
                    .Select(run => run.StatisticDate)
                    .ToListAsync())
                .Select(date => date.Date)
                .Distinct()
                .ToList();

            var completedDateSet = completedDates.ToHashSet();
            var hourlyStatistics = await db.RemoteHourlyStatistics
                .AsNoTracking()
                .Where(statistic =>
                    statistic.FK_WebsiteId == siteId
                    && statistic.StatisticHour >= rangeStart
                    && statistic.StatisticHour < rangeEnd.AddDays(1))
                .Select(statistic => new
                {
                    statistic.StatisticHour,
                    statistic.EffectiveViews,
                    statistic.EffectiveUniqueVisitors,
                    statistic.AggregatedAt
                })
                .ToListAsync();

            var completedStatistics = hourlyStatistics
                .Where(statistic => completedDateSet.Contains(statistic.StatisticHour.Date))
                .ToList();
            var statisticsBySlot = completedStatistics
                .GroupBy(statistic => new
                {
                    DayOfWeek = ToDashboardDayOfWeek(statistic.StatisticHour.DayOfWeek),
                    statistic.StatisticHour.Hour
                })
                .ToDictionary(
                    group => (group.Key.DayOfWeek, group.Key.Hour),
                    group => new
                    {
                        Visitors = group.Sum(item => item.EffectiveUniqueVisitors),
                        Views = group.Sum(item => item.EffectiveViews)
                    });
            var sampleDaysByWeekday = completedDates
                .GroupBy(date => ToDashboardDayOfWeek(date.DayOfWeek))
                .ToDictionary(group => group.Key, group => group.Count());

            var items = new List<DashboardTrafficHeatmapCellDto>(7 * 24);
            for (var dayOfWeek = 1; dayOfWeek <= 7; dayOfWeek++)
            {
                sampleDaysByWeekday.TryGetValue(dayOfWeek, out var sampleDays);
                for (var hour = 0; hour < 24; hour++)
                {
                    statisticsBySlot.TryGetValue((dayOfWeek, hour), out var statistic);
                    var totalVisitors = statistic?.Visitors ?? 0;
                    var totalViews = statistic?.Views ?? 0;
                    items.Add(new DashboardTrafficHeatmapCellDto
                    {
                        DayOfWeek = dayOfWeek,
                        Hour = hour,
                        SampleDays = sampleDays,
                        TotalVisitors = totalVisitors,
                        TotalViews = totalViews,
                        AverageVisitors = sampleDays == 0
                            ? 0
                            : Math.Round((double)totalVisitors / sampleDays, 1),
                        AverageViews = sampleDays == 0
                            ? 0
                            : Math.Round((double)totalViews / sampleDays, 1)
                    });
                }
            }

            var recommendedSlots = items
                .Where(item => item.SampleDays > 0 && item.AverageVisitors > 0)
                .OrderByDescending(item => item.AverageVisitors)
                .ThenByDescending(item => item.AverageViews)
                .ThenBy(item => item.DayOfWeek)
                .ThenBy(item => item.Hour)
                .Take(5)
                .Select(item => new DashboardRecommendedTrafficSlotDto
                {
                    DayOfWeek = item.DayOfWeek,
                    Hour = item.Hour,
                    SampleDays = item.SampleDays,
                    AverageVisitors = item.AverageVisitors,
                    AverageViews = item.AverageViews
                })
                .ToList();

            return new DashboardTrafficHeatmapOutputDto
            {
                Days = days,
                StartDate = rangeStart,
                EndDate = rangeEnd,
                AvailableDays = completedDates.Count,
                UpdatedAt = completedStatistics.Count == 0
                    ? null
                    : completedStatistics.Max(item => item.AggregatedAt),
                Items = items,
                RecommendedSlots = recommendedSlots
            };
        }

        private static int ToDashboardDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek;
        }

        public async Task<List<DashboardPopularPageDto>> GetPopularPages(
            int take = 5,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            take = Math.Clamp(take, 1, 10);
            var siteId = await loginUserData.GetWebsiteId();
            var today = DateTime.Today;
            var rangeStart = startDate?.Date ?? today;
            var rangeEnd = endDate?.Date ?? today;

            var pages =
                from statistic in db.RemoteDailyStatistics.AsNoTracking()
                join menu in db.WebMenus.AsNoTracking()
                    on statistic.FK_WebmenuId equals menu.Id
                join articleSource in db.Article.AsNoTracking()
                    on statistic.FK_ArticleId equals articleSource.Id into articles
                from article in articles.DefaultIfEmpty()
                join productSource in db.Prods.AsNoTracking()
                    on statistic.FK_ProdId equals productSource.Id into products
                from product in products.DefaultIfEmpty()
                join certificateSource in db.TechnicalCertificates.AsNoTracking()
                    on statistic.FK_TechCertId equals certificateSource.Id into certificates
                from certificate in certificates.DefaultIfEmpty()
                where statistic.FK_WebsiteId == siteId
                    && statistic.StatisticDate >= rangeStart
                    && statistic.StatisticDate <= rangeEnd
                    && statistic.Scope == 1
                    && statistic.FK_WebmenuId > 0
                    && !menu.IsDeleted
                select new
                {
                    WebMenuId = menu.Id,
                    Title = article != null && article.Title != null
                        ? article.Title
                        : product != null
                            ? product.Title
                            : certificate != null && certificate.Title != null
                                ? certificate.Title
                                : menu.Title ?? "未命名頁面",
                    ContentType = article != null
                        ? "文章"
                        : product != null
                            ? "商品"
                            : certificate != null
                                ? "證書"
                                : "頁面",
                    Views = statistic.EffectiveViews,
                    Visitors = statistic.EffectiveUniqueVisitors
                };

            return await pages
                .GroupBy(page => new
                {
                    page.WebMenuId,
                    page.Title,
                    page.ContentType
                })
                .Select(group => new DashboardPopularPageDto
                {
                    WebMenuId = group.Key.WebMenuId,
                    Title = group.Key.Title,
                    ContentType = group.Key.ContentType,
                    Views = group.Sum(page => page.Views),
                    Visitors = group.Sum(page => page.Visitors)
                })
                .OrderByDescending(page => page.Views)
                .ThenByDescending(page => page.Visitors)
                .Take(take)
                .ToListAsync();
        }

        public async Task<DashboardContactsOutputDto> GetContacts(int take = 5)
        {
            take = Math.Clamp(take, 1, 10);
            var siteId = await loginUserData.GetWebsiteId();
            var contacts = db.Contacts
                .AsNoTracking()
                .Where(contact =>
                    !contact.IsDeleted
                    && contact.WebMenu.FK_WebsiteId == siteId
                    && !contact.WebMenu.IsDeleted);
            var formStatusContacts = contacts
                .Where(contact => contact.Status != ContactStatusEnum.作廢_忽略);

            var grouped = await formStatusContacts
                .GroupBy(contact => new { contact.Name, contact.Status })
                .Select(group => new
                {
                    group.Key.Name,
                    group.Key.Status,
                    Count = group.LongCount()
                })
                .ToListAsync();

            var forms = grouped
                .GroupBy(item => string.IsNullOrWhiteSpace(item.Name) ? "未命名表單" : item.Name)
                .Select(group => new DashboardContactFormSummaryDto
                {
                    Name = group.Key,
                    PendingCount = group
                        .Where(item => item.Status == ContactStatusEnum.未處理)
                        .Sum(item => item.Count),
                    ProcessingCount = group
                        .Where(item => item.Status == ContactStatusEnum.處理中)
                        .Sum(item => item.Count),
                    RepliedCount = group
                        .Where(item => item.Status == ContactStatusEnum.已回覆)
                        .Sum(item => item.Count),
                    CompletedCount = group
                        .Where(item => item.Status == ContactStatusEnum.已完成)
                        .Sum(item => item.Count)
                })
                .OrderByDescending(item => item.PendingCount)
                .ThenByDescending(item => item.ProcessingCount)
                .ThenBy(item => item.Name)
                .ToList();

            var recentSource = await formStatusContacts
                .Where(contact =>
                    contact.Status != ContactStatusEnum.已完成)
                .OrderByDescending(contact => contact.CreationTime)
                .Select(contact => new
                {
                    contact.Id,
                    contact.Name,
                    contact.UserName,
                    contact.Status,
                    contact.CreationTime
                })
                .Take(take)
                .ToListAsync();

            return new DashboardContactsOutputDto
            {
                HasData = forms.Count > 0,
                PendingCount = forms.Sum(item => item.PendingCount),
                Forms = forms,
                Recent = recentSource
                    .Select(contact => new DashboardRecentContactDto
                    {
                        Id = contact.Id,
                        FormName = string.IsNullOrWhiteSpace(contact.Name)
                            ? "未命名表單"
                            : contact.Name,
                        UserName = string.IsNullOrWhiteSpace(contact.UserName)
                            ? "未填寫姓名"
                            : stringHandler.MaskName(contact.UserName),
                        Status = contact.Status.ToString().Replace("_", "/"),
                        CreationTime = contact.CreationTime
                    })
                    .ToList(),
                UpdatedAt = DateTime.Now
            };
        }

        public async Task<DashboardCommerceOverviewDto> GetCommerceOverview()
        {
            var siteId = await loginUserData.GetWebsiteId();
            var level = await loginUserData.GetWebsiteLevel(siteId);
            if (!await IsCommerceEnabled(siteId, level))
            {
                return new DashboardCommerceOverviewDto
                {
                    IsCommerceEnabled = false
                };
            }

            var trackedOrderStates = new[]
            {
                OrderStatusEnum.待確認,
                OrderStatusEnum.待付款,
                OrderStatusEnum.已付款,
                OrderStatusEnum.已出貨
            };
            var paidOrderStates = new[]
            {
                OrderStatusEnum.已付款,
                OrderStatusEnum.已出貨,
                OrderStatusEnum.已完成
            };
            var hasOrderData = await db.Order_Headers
                .AsNoTracking()
                .AnyAsync(order =>
                    order.FK_WebsiteId == siteId
                    && !order.IsDeleted
                    && !order.IsTemp);
            var orderCounts = await db.Order_Headers
                .AsNoTracking()
                .Where(order =>
                    order.FK_WebsiteId == siteId
                    && !order.IsDeleted
                    && !order.IsTemp
                    && trackedOrderStates.Contains(order.State))
                .GroupBy(order => order.State)
                .Select(group => new
                {
                    State = group.Key,
                    Count = group.LongCount()
                })
                .ToDictionaryAsync(item => item.State, item => item.Count);
            var recentOrderSource = await db.Order_Headers
                .AsNoTracking()
                .Where(order =>
                    order.FK_WebsiteId == siteId
                    && !order.IsDeleted
                    && !order.IsTemp
                    && trackedOrderStates.Contains(order.State))
                .OrderByDescending(order => order.CreationTime)
                .Select(order => new
                {
                    order.Id,
                    order.Orderer,
                    order.State,
                    Total = order.Subtotal + order.Freight,
                    order.CreationTime
                })
                .Take(5)
                .ToListAsync();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var yesterday = today.AddDays(-1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var previousMonthStart = monthStart.AddMonths(-1);
            var previousMonthDays = DateTime.DaysInMonth(
                previousMonthStart.Year,
                previousMonthStart.Month);
            var previousMonthComparisonEnd = previousMonthStart.AddDays(
                Math.Min(today.Day, previousMonthDays));
            var paidOrders = await db.Order_Headers
                .AsNoTracking()
                .Where(order =>
                    order.FK_WebsiteId == siteId
                    && !order.IsDeleted
                    && !order.IsTemp
                    && paidOrderStates.Contains(order.State)
                    && order.CreationTime >= previousMonthStart
                    && order.CreationTime < tomorrow)
                .Select(order => new
                {
                    order.CreationTime,
                    Amount = order.Subtotal + order.Freight
                })
                .ToListAsync();

            var todayPaidOrders = paidOrders
                .Where(order =>
                    order.CreationTime >= today
                    && order.CreationTime < tomorrow)
                .ToList();
            var monthPaidOrders = paidOrders
                .Where(order =>
                    order.CreationTime >= monthStart
                    && order.CreationTime < tomorrow)
                .ToList();

            var lowStockCount = await db.Prod_Stocks
                .AsNoTracking()
                .LongCountAsync(stock =>
                    !stock.IsDeleted
                    && !stock.IsTimePrice
                    && stock.Stock.HasValue
                    && stock.Alert_Qty.HasValue
                    && stock.Stock.Value <= stock.Alert_Qty.Value
                    && stock.Prod != null
                    && stock.Prod.FK_WebsiteId == siteId
                    && !stock.Prod.IsDeleted
                    && stock.Prod.Visible
                    && !stock.Prod.RemovedFromShelves
                    && !stock.Prod.NoStockManagement);

            var soldOutProductCount = await db.Prods
                .AsNoTracking()
                .LongCountAsync(product =>
                    product.FK_WebsiteId == siteId
                    && !product.IsDeleted
                    && product.Visible
                    && !product.RemovedFromShelves
                    && product.Status == ProdStatusEnum.售完);

            return new DashboardCommerceOverviewDto
            {
                IsCommerceEnabled = true,
                HasOrderData = hasOrderData,
                PendingConfirmationCount = GetOrderCount(orderCounts, OrderStatusEnum.待確認),
                PendingPaymentCount = GetOrderCount(orderCounts, OrderStatusEnum.待付款),
                AwaitingShipmentCount = GetOrderCount(orderCounts, OrderStatusEnum.已付款),
                ShippingCount = GetOrderCount(orderCounts, OrderStatusEnum.已出貨),
                LowStockCount = lowStockCount,
                SoldOutProductCount = soldOutProductCount,
                TodayOrderAmount = todayPaidOrders.Sum(order => order.Amount),
                YesterdayOrderAmount = paidOrders
                    .Where(order =>
                        order.CreationTime >= yesterday
                        && order.CreationTime < today)
                    .Sum(order => order.Amount),
                MonthOrderAmount = monthPaidOrders.Sum(order => order.Amount),
                PreviousMonthOrderAmount = paidOrders
                    .Where(order =>
                        order.CreationTime >= previousMonthStart
                        && order.CreationTime < previousMonthComparisonEnd)
                    .Sum(order => order.Amount),
                TodayPaidOrderCount = todayPaidOrders.LongCount(),
                MonthPaidOrderCount = monthPaidOrders.LongCount(),
                RecentOrders = recentOrderSource
                    .Select(order => new DashboardRecentOrderDto
                    {
                        Id = order.Id,
                        Orderer = string.IsNullOrWhiteSpace(order.Orderer)
                            ? "未填寫訂購人"
                            : stringHandler.MaskName(order.Orderer),
                        Status = order.State.ToString(),
                        Total = order.Total,
                        CreationTime = order.CreationTime
                    })
                    .ToList()
            };
        }

        public async Task<DashboardOrderTrendOutputDto> GetOrderTrend(int days = 30)
        {
            if (days != 7 && days != 30)
                throw new ArgumentOutOfRangeException(nameof(days), "訂單趨勢僅支援近 7 天或近 30 天。");

            var siteId = await loginUserData.GetWebsiteId();
            var level = await loginUserData.GetWebsiteLevel(siteId);
            if (!await IsCommerceEnabled(siteId, level))
            {
                return new DashboardOrderTrendOutputDto
                {
                    IsCommerceEnabled = false,
                    Days = days
                };
            }

            var rangeEnd = DateTime.Today;
            var rangeStart = rangeEnd.AddDays(-(days - 1));
            var nextDate = rangeEnd.AddDays(1);
            var groupedOrders = await db.Order_Headers
                .AsNoTracking()
                .Where(order =>
                    order.FK_WebsiteId == siteId
                    && !order.IsDeleted
                    && !order.IsTemp
                    && order.State != OrderStatusEnum.已取消
                    && order.State != OrderStatusEnum.付款失敗
                    && order.CreationTime >= rangeStart
                    && order.CreationTime < nextDate)
                .GroupBy(order => order.CreationTime.Date)
                .Select(group => new
                {
                    Date = group.Key,
                    OrderCount = group.LongCount(),
                    Revenue = group.Sum(order => order.Subtotal + order.Freight)
                })
                .ToListAsync();

            var ordersByDate = groupedOrders.ToDictionary(item => item.Date);
            return new DashboardOrderTrendOutputDto
            {
                IsCommerceEnabled = true,
                Days = days,
                StartDate = rangeStart,
                EndDate = rangeEnd,
                Items = Enumerable.Range(0, days)
                    .Select(offset =>
                    {
                        var date = rangeStart.AddDays(offset);
                        ordersByDate.TryGetValue(date, out var item);
                        return new DashboardOrderTrendItemDto
                        {
                            Date = date,
                            OrderCount = item?.OrderCount ?? 0,
                            Revenue = item?.Revenue ?? 0
                        };
                    })
                    .ToList()
            };
        }

        private async Task<bool> IsCommerceEnabled(
            long siteId,
            WebsiteLevelEnum level)
        {
            if (level < WebsiteLevelEnum.購物)
                return false;

            var storeBuyState = await db.StoreSetDetail
                .AsNoTracking()
                .Where(detail =>
                    detail.FK_WebsiteId == siteId
                    && !detail.IsDeleted
                    && detail.StoreSet != null
                    && !detail.StoreSet.IsDeleted
                    && detail.StoreSet.key == "storeBuyState")
                .OrderByDescending(detail => detail.CreationTime)
                .Select(detail => detail.value)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(storeBuyState))
                return false;

            return storeBuyState
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(state =>
                    string.Equals(state, "Pay", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(state, "menberPay", StringComparison.OrdinalIgnoreCase));
        }

        private static long GetOrderCount(
            IReadOnlyDictionary<OrderStatusEnum, long> counts,
            OrderStatusEnum status)
        {
            return counts.TryGetValue(status, out var count) ? count : 0;
        }

        private static string GetLastChangeDate(string directoryPath)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                return directoryInfo.Exists
                    ? directoryInfo.LastWriteTime.ToString("yyyy/M/d tt hh:mm:ss")
                    : "尚未建立目錄";
            }
            catch
            {
                return "無法取得";
            }
        }

        private static string CalculateDirectorySize(string directoryPath)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                if (!directoryInfo.Exists)
                    return "尚未建立目錄";

                var totalSizeInBytes = directoryInfo
                    .EnumerateFiles("*", SearchOption.AllDirectories)
                    .Sum(file => file.Length);

                return FormatSize(totalSizeInBytes);
            }
            catch
            {
                return "無法取得";
            }
        }

        private static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double length = bytes;
            var order = 0;

            while (length >= 1024 && order < sizes.Length - 1)
            {
                order++;
                length /= 1024;
            }

            return $"{length:0.##} {sizes[order]}";
        }

        private static void AddParameter(
            IDbCommand command,
            string name,
            object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }
    }
}
