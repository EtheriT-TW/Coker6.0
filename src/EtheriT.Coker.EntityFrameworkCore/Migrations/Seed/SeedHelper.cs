using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public class SeedHelper
    {
        private readonly ModelBuilder modelBuilder;
        public SeedHelper(ModelBuilder modelBuilder)
        {
            this.modelBuilder = modelBuilder;
        }
        public void SeedHost()
        {
            modelBuilder.Entity<User>().HasData(
               new User()
               {
                   Id = 1,
                   Name = "易碩網際科技科技股份有限公司",
                   Account = "EtheriT",
                   CellPhone = "0906801568",
                   Email = "service@ether.com.tw",
                   Password = "AQAAAAEAACcQAAAAEE3X/SrNcUs6zaH9K+51XEMp8G2z3r9d/5SYuLJpKy3TlYNX7DdHF6PDW8NxWk7CWg==", //123qwe
                   CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1328)
               }, new User()
               {
                   Id = 2,
                   Name = "隆昌窯業",
                   Account = "lcb",
                   CellPhone = "0920497649",
                   Email = "lienmienchou@evergreen.com.tw",
                   Password = "AQAAAAEAACcQAAAAEE3X/SrNcUs6zaH9K+51XEMp8G2z3r9d/5SYuLJpKy3TlYNX7DdHF6PDW8NxWk7CWg==", //123qwe
                   CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1338)
               }
            );
            modelBuilder.Entity<Website>().HasData(
                new Website()
                {
                    Id = 1,
                    Title = "Coker雲端開店大師",
                    Locale = "zh-tw",
                    Type = "website",
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1441),
                }, new Website()
                {
                    Id = 2,
                    Title = "｜Derek｜德瑞克．隆昌窯業",
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
            modelBuilder.Entity<Prod_Spec_Type>().HasData(
                new Prod_Spec_Type()
                {
                    Id = 1,
                    FK_WebsiteId = 2,
                    Type = "color",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Spec_Type()
                {
                    Id = 2,
                    FK_WebsiteId = 2,
                    Type = "size",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1460),
                }
            );
            modelBuilder.Entity<Prod_Spec>().HasData(
                new Prod_Spec()
                {
                    Id = 1,
                    FK_Tid = 1,
                    Title = "white",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1461),
                },
                new Prod_Spec()
                {
                    Id = 2,
                    FK_Tid = 1,
                    Title = "gray",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1462),
                },
                new Prod_Spec()
                {
                    Id = 3,
                    FK_Tid = 1,
                    Title = "black",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1463),
                },
                new Prod_Spec()
                {
                    Id = 4,
                    FK_Tid = 2,
                    Title = "small",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1464),
                },
                new Prod_Spec()
                {
                    Id = 5,
                    FK_Tid = 2,
                    Title = "medium",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1465),
                },
                new Prod_Spec()
                {
                    Id = 6,
                    FK_Tid = 2,
                    Title = "large",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466),
                }
            );
            modelBuilder.Entity<Prod>().HasData(
                new Prod()
                {
                    Id = 1,
                    FK_WebsiteId = 2,
                    Title = "商品一的名稱",
                    Disp_Opt = true,
                    Ser_No = 500,
                    Introduction = "商品一的介紹",
                    Description = "商品一的說明",
                    Price = 28000,
                    CreatorUserId = 2,
                    permanent = false,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod()
                {
                    Id = 2,
                    FK_WebsiteId = 2,
                    Title = "商品二的名稱",
                    Disp_Opt = true,
                    Ser_No = 500,
                    Introduction = "商品二的介紹",
                    Description = "商品二的說明",
                    Price = 9500,
                    CreatorUserId = 2,
                    permanent = false,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod()
                {
                    Id = 3,
                    FK_WebsiteId = 2,
                    Title = "商品三的名稱",
                    Disp_Opt = true,
                    Ser_No = 500,
                    Introduction = "商品三的介紹",
                    Description = "商品三的說明",
                    Price = 13000,
                    CreatorUserId = 2,
                    permanent = false,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                }
            );
            modelBuilder.Entity<Prod_Stock>().HasData(
                new Prod_Stock()
                {
                    Id = 1,
                    FK_Pid = 1,
                    Stock = 100,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 2,
                    FK_Pid = 2,
                    Stock = 100,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 3,
                    FK_Pid = 3,
                    Stock = 100,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                }
            );
        }
    }
}
