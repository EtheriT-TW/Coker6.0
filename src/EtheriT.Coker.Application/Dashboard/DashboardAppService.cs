using EtheriT.Coker.Application.Shared;
using EtheriT.Coker.Application.Shared.Dashboard;
using EtheriT.Coker.Application.Shared.Dto.Dashboard;
using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.FlowSize;
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

        public DashboardAppService(
            CokerDbContext db,
            LoginUserData loginUserData,
            IFlowSizeAppService flowSizeAppService,
            IUploadPathResolver uploadPathResolver)
        {
            this.db = db;
            this.loginUserData = loginUserData;
            this.flowSizeAppService = flowSizeAppService;
            this.uploadPathResolver = uploadPathResolver;
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
            const string commandText =
                """
                WITH [EffectiveRemote] AS
                (
                    SELECT
                        DATEPART(hour, [remote].[ExecutionTime]) AS [StatisticHour],
                        CASE
                            WHEN [remote].[FK_UserId] IS NOT NULL
                                THEN CONCAT('user:', [remote].[FK_UserId])
                            WHEN [remote].[UUID] <> '00000000-0000-0000-0000-000000000000'
                                THEN CONCAT('uuid:', CONVERT(varchar(36), [remote].[UUID]))
                            ELSE CONCAT('ip:', ISNULL([remote].[ClientIpAddress], ''))
                        END AS [VisitorIdentifier],
                        [remote].[LastStatComputedAt]
                    FROM [dbo].[Remotes] AS [remote]
                    WHERE [remote].[FK_WebsiteId] = @WebsiteId
                      AND [remote].[ExecutionTime] >= @StartDate
                      AND [remote].[ExecutionTime] < DATEADD(day, 1, @StartDate)
                      AND
                      (
                          [remote].[IsEngaged] = CAST(1 AS bit)
                          OR
                          (
                              [remote].[TrackingEventId] IS NULL
                              AND [remote].[TimeOnPage] > 0
                              AND [remote].[State] <> 2
                          )
                      )
                )
                SELECT
                    [source].[StatisticHour],
                    COUNT_BIG(*) AS [EffectiveViews],
                    COUNT_BIG(DISTINCT [source].[VisitorIdentifier]) AS [EffectiveUniqueVisitors],
                    MAX([source].[LastStatComputedAt]) AS [UpdatedAt]
                FROM [EffectiveRemote] AS [source]
                GROUP BY [source].[StatisticHour]
                ORDER BY [source].[StatisticHour];
                """;

            var hourlyStatistics = new Dictionary<int, (long Views, long Visitors)>();
            DateTime? updatedAt = null;
            var connection = db.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;

            if (shouldClose)
                await connection.OpenAsync();

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = commandText;

                AddParameter(command, "@WebsiteId", siteId);
                AddParameter(command, "@StartDate", statisticDate.Date);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var hour = reader.GetInt32(0);
                    hourlyStatistics[hour] = (reader.GetInt64(1), reader.GetInt64(2));

                    if (!reader.IsDBNull(3))
                    {
                        var itemUpdatedAt = reader.GetDateTime(3);
                        if (!updatedAt.HasValue || itemUpdatedAt > updatedAt.Value)
                            updatedAt = itemUpdatedAt;
                    }
                }
            }
            finally
            {
                if (shouldClose)
                    await connection.CloseAsync();
            }

            return new DashboardTrafficOutputDto
            {
                Granularity = "hour",
                Items = Enumerable.Range(0, 24)
                    .Select(hour =>
                    {
                        hourlyStatistics.TryGetValue(hour, out var statistic);
                        return new DashboardTrafficItemDto
                        {
                            Date = statisticDate.AddHours(hour),
                            PageViews = statistic.Views,
                            Visitors = statistic.Visitors
                        };
                    })
                    .ToList(),
                UpdatedAt = updatedAt ?? DateTime.Now
            };
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

            var grouped = await contacts
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

            var recentSource = await contacts
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
                            : contact.UserName,
                        Status = contact.Status.ToString().Replace("_", "/"),
                        CreationTime = contact.CreationTime
                    })
                    .ToList(),
                UpdatedAt = DateTime.Now
            };
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
