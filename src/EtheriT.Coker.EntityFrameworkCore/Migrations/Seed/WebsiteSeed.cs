using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public static class WebsiteSeed
    {
        public static void Seed(ModelBuilder modelBuilder) {

            modelBuilder.Entity<Website>().HasData(
                new Website()
                {
                    Id = 1,
                    Title = "Coker雲端開店大師",
                    OrgName = "coker6",
                    Locale = "zh-tw",
                    Type = "website",
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1441),
                }, new Website()
                {
                    Id = 2,
                    Title = "｜Derek｜德瑞克．隆昌窯業",
                    OrgName = "lcb",
                    Locale = "zh-tw",
                    Type = "website",
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1443),
                }
            );
            modelBuilder.Entity<MappingUserAndWebsite>().HasData(
                new MappingUserAndWebsite()
                {
                    Id = 1,
                    UserId = 1,
                    WebsiteId = 1,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1457),
                }, new MappingUserAndWebsite()
                {
                    Id = 2,
                    UserId = 2,
                    WebsiteId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1458),
                }
            );
        }
    }
}
