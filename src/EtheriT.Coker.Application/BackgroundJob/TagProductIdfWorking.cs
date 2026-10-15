using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.EntityFrameworkCore.EntityFrameworkCore;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtheriT.Coker.Application.BackgroundJob
{
    public sealed class TagProductIdfWorking
    {
        private readonly CokerDbContext db;
        private readonly ILogger<TagProductIdfWorking> logger;

        public TagProductIdfWorking(CokerDbContext db, ILogger<TagProductIdfWorking> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [AutomaticRetry(Attempts = 1)]
        [DisableConcurrentExecution(3600)]
        public async Task Recalculate()
        {
            var updatedAtUtc = DateTime.UtcNow;
            // 每站獨立計算所有未刪除商品，不受上下架、庫存或目錄標籤影響。
            // 使用集合更新，避免將所有商品與綁定資料載入記憶體。
            var updatedTags = await db.Database.ExecuteSqlInterpolatedAsync($"""
                ;WITH [ProductTotals] AS
                (
                    SELECT [FK_WebsiteId], COUNT_BIG(*) AS [TotalProducts]
                    FROM [Prods]
                    WHERE [IsDeleted] = 0
                    GROUP BY [FK_WebsiteId]
                ),
                [TagProductCounts] AS
                (
                    SELECT [tag].[Id] AS [TagId], COUNT_BIG(DISTINCT [prod].[Id]) AS [ProductCount]
                    FROM [Tag_Associates] AS [binding]
                    INNER JOIN [Tags] AS [tag] ON [tag].[Id] = [binding].[FK_TId]
                    INNER JOIN [Prods] AS [prod]
                        ON [prod].[Id] = [binding].[FK_AId]
                        AND [prod].[FK_WebsiteId] = [tag].[FK_WebsiteId]
                    WHERE [binding].[Type] = {(int)TagAssociateTypeEnum.商品}
                        AND [binding].[IsDeleted] = 0
                        AND [prod].[IsDeleted] = 0
                        AND [tag].[IsDeleted] = 0
                        AND [tag].[IsTemporary] = 0
                    GROUP BY [tag].[Id]
                )
                UPDATE [tag]
                SET [ProductIdfWeight] = LOG(
                        (CONVERT(float, ISNULL([total].[TotalProducts], 0)) + 1.0)
                        / (CONVERT(float, ISNULL([counts].[ProductCount], 0)) + 1.0)
                    ) + 1.0,
                    [ProductIdfUpdatedAtUtc] = {updatedAtUtc}
                FROM [Tags] AS [tag]
                LEFT JOIN [ProductTotals] AS [total] ON [total].[FK_WebsiteId] = [tag].[FK_WebsiteId]
                LEFT JOIN [TagProductCounts] AS [counts] ON [counts].[TagId] = [tag].[Id]
                WHERE [tag].[IsDeleted] = 0 AND [tag].[IsTemporary] = 0;
                """);

            logger.LogInformation(
                "Product tag IDF recalculated. UpdatedTags={UpdatedTags}, UpdatedAtUtc={UpdatedAtUtc}",
                updatedTags,
                updatedAtUtc);
        }
    }
}
