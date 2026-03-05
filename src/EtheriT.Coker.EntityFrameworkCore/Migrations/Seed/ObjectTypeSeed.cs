using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public static class ObjectTypeSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ObjectType>().HasData(
                new ObjectType
                {
                    Id = 1,
                    Title = "目錄",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 2,
                    Title = "廣告",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 3,
                    Title = "編排樣式",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 4,
                    Title = "電子報樣版",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 10, 27, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 5,
                    Title = "樣版",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2024, 7, 12, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 6,
                    Title = "框架",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2024, 7, 12, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 7,
                    Title = "廣告Banner",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2024, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 8,
                    Title = "標題設計",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2024, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 9,
                    Title = "按鈕設計",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2024, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 10,
                    Title = "多欄位編排",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2024, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 11,
                    Title = "進階",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2024, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 12,
                    Title = "廣告(加購項目)",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2024, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 13,
                    Title = "文章樣板",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2026, 3, 4, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 99,
                    Title = "小工具",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 999,
                    Title = "自訂",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }
            );
        }
    }
}
