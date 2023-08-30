using EtheriT.Coker.Core.Models;
using EtheriT.Coker.Web.Core.Models;
using Microsoft.EntityFrameworkCore;
using static System.Formats.Asn1.AsnWriter;

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
            modelBuilder.Entity<Prod_Spec_Type>().HasData(
                new Prod_Spec_Type()
                {
                    Id = 1,
                    FK_WebsiteId = 2,
                    Type = "顏色",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Spec_Type()
                {
                    Id = 2,
                    FK_WebsiteId = 2,
                    Type = "尺寸",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Spec_Type()
                {
                    Id = 3,
                    FK_WebsiteId = 2,
                    Type = "其他",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                }
            );
            modelBuilder.Entity<Prod_Spec>().HasData(
                new Prod_Spec()
                {
                    Id = 1,
                    FK_Tid = 1,
                    Title = "白色",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1461),
                },
                new Prod_Spec()
                {
                    Id = 2,
                    FK_Tid = 1,
                    Title = "灰色",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1462),
                },
                new Prod_Spec()
                {
                    Id = 3,
                    FK_Tid = 1,
                    Title = "黑色",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1463),
                },
                new Prod_Spec()
                {
                    Id = 4,
                    FK_Tid = 2,
                    Title = "小",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1464),
                },
                new Prod_Spec()
                {
                    Id = 5,
                    FK_Tid = 2,
                    Title = "中",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1465),
                },
                new Prod_Spec()
                {
                    Id = 6,
                    FK_Tid = 2,
                    Title = "大",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466),
                },
                new Prod_Spec()
                {
                    Id = 7,
                    FK_Tid = 3,
                    Title = "整組",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466),
                },
                new Prod_Spec()
                {
                    Id = 8,
                    FK_Tid = 3,
                    Title = "L183NA 檯上奈米方型盆",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466),
                },
                new Prod_Spec()
                {
                    Id = 9,
                    FK_Tid = 3,
                    Title = "1033PH 四角型單孔單槍加高面盆龍頭",
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1466),
                }
            );
            modelBuilder.Entity<Prod>().HasData(
                new Prod()
                {
                    Id = 1,
                    FK_WebsiteId = 2,
                    Title = "DE-R1073 德瑞克直熱式微電腦馬桶座／遙控型",
                    Disp_Opt = true,
                    Ser_No = 500,
                    Introduction = "從座圈到噴嘴給您雙重防護\n不用動手全自動科技最體貼\n雙漩洗技術為您實現真乾淨",
                    Description = "奈米單體馬桶 W384 x D685 x H470mm\n直熱式微電腦馬桶座\n噴嘴紫外線殺菌\n獨立水壓系統\n腳觸設計\nEasy Touch開閉蓋技術\n第二代微波感應技術",
                    CreatorUserId = 2,
                    permanent = true,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod()
                {
                    Id = 2,
                    FK_WebsiteId = 2,
                    Title = "C659NA 德瑞克Smart III淨未來智慧馬桶",
                    Disp_Opt = true,
                    Ser_No = 500,
                    Introduction = "商品二的第一行介紹\n商品二的第二行介紹",
                    Description = "商品二的第一行說明\n商品二的第二行說明",
                    CreatorUserId = 2,
                    permanent = true,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod()
                {
                    Id = 3,
                    FK_WebsiteId = 2,
                    Title = "L602 檯上三角盆",
                    Disp_Opt = true,
                    Ser_No = 500,
                    Introduction = "商品三的第一行介紹\n商品二的第二行介紹",
                    Description = "商品三的第一行說明\n商品二的第二行說明",
                    CreatorUserId = 2,
                    permanent = true,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod()
                {
                    Id = 4,
                    FK_WebsiteId = 2,
                    Title = "L183NA 檯上奈米方型盆",
                    Disp_Opt = true,
                    Ser_No = 500,
                    Introduction = "最大容水量：11公升\n適用水壓：1~5kgf/㎝²",
                    Description = "L183NA檯上奈米方型盆W560 x D380 x H120mm\n1033PH四角型單孔單槍加高面盆龍頭歐洲省水二段Ø35短腳陶瓷心軸(附歐規按押無溢水排桿)",
                    CreatorUserId = 2,
                    permanent = true,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                }
            );
            modelBuilder.Entity<Prod_Stock>().HasData(
                new Prod_Stock()
                {
                    Id = 1,
                    FK_Pid = 1,
                    FK_S1id = 1,
                    FK_S2id = 4,
                    Stock = 100,
                    Alert_Qty = 5,
                    Min_Qty = 1,
                    Price = 30000,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 2,
                    FK_Pid = 1,
                    FK_S1id = 2,
                    FK_S2id = 4,
                    Stock = 100,
                    Alert_Qty = 5,
                    Min_Qty = 1,
                    Price = 28000,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 3,
                    FK_Pid = 1,
                    FK_S1id = 2,
                    FK_S2id = 5,
                    Stock = 100,
                    Alert_Qty = 5,
                    Min_Qty = 1,
                    Price = 28500,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 4,
                    FK_Pid = 2,
                    FK_S1id = 1,
                    FK_S2id = 4,
                    Stock = 100,
                    Alert_Qty = 5,
                    Min_Qty = 1,
                    Price = 9500,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 5,
                    FK_Pid = 3,
                    FK_S1id = 1,
                    FK_S2id = 4,
                    Stock = 100,
                    Alert_Qty = 5,
                    Min_Qty = 1,
                    Price = 13000,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 6,
                    FK_Pid = 4,
                    FK_S1id = 7,
                    FK_S2id = 0,
                    Stock = 100,
                    Alert_Qty = 5,
                    Min_Qty = 1,
                    Price = 24300,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 7,
                    FK_Pid = 4,
                    FK_S1id = 8,
                    FK_S2id = 0,
                    Stock = 100,
                    Alert_Qty = 5,
                    Min_Qty = 1,
                    Price = 9500,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                },
                new Prod_Stock()
                {
                    Id = 8,
                    FK_Pid = 4,
                    FK_S1id = 9,
                    FK_S2id = 0,
                    Stock = 100,
                    Alert_Qty = 5,
                    Min_Qty = 1,
                    Price = 14800,
                    Ser_No = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2022, 11, 7, 17, 52, 57, 552, DateTimeKind.Local).AddTicks(1459),
                }
            );
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
                    Title = "編排",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 8,
                    Title = "進入廣告",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 12,
                    Title = "浮動廣告",
                    SerNo = 500,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ObjectType
                {
                    Id = 99,
                    Title = "更多",
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

            modelBuilder.Entity<StoreSet>().HasData(
                new StoreSet
                {
                    Id = 1,
                    key = "Google",
                    name = "Google Analytics(4)",
                    memo = "請輸入GOOGLE提供之驗證碼：xxxxxx-x",
                    groupType = 1,
                    type = 1,
                    maxlength = 8,
                    pattern = $@"\d{6}-\d",
                    IsDeleted = false,
                    CreatorUserId = 2,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );
        }
    }
}
