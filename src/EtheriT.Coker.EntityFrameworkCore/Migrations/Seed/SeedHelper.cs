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
                    Visible = true,
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
                    Visible = true,
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
                    Visible = true,
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
                    Visible = true,
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
            modelBuilder.Entity<StoreSetGroup>().HasData(
                new StoreSetGroup
                {
                    Id = 1,
                    Title = "Google設定",
                    Image = "/images/icon_google.png",
                    Description = "",
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSetGroup
                {
                    Id = 2,
                    Title = "商店設定",
                    Image = "",
                    Description = "",
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 26, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );

            modelBuilder.Entity<StoreSet>().HasData(
                new StoreSet
                {
                    Id = 1,
                    key = "GA4",
                    name = "Google Analytics(4)",
                    memo = "請輸入GOOGLE提供之驗證碼：G-xxxxxxxxxx",
                    FK_StoreSetGroupId = 1,
                    type = 1,
                    maxlength = 12,
                    pattern = "^G-\\w+",
                    IsDeleted = false,
                    jobID = "S001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 2,
                    key = "google.translate",
                    name = "Google自動翻譯",
                    memo = "請選擇需要翻譯的語系",
                    FK_StoreSetGroupId = 1,
                    type = 4,
                    maxlength = 50,
                    pattern = "(?=[a-z]{2}-?[A-Z]{0,2},?)+",
                    IsDeleted = false,
                    jobID = "S002",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 3,
                    key = "storeBuyState",
                    name = "商品販售設定",
                    memo = "請選擇購物形式",
                    FK_StoreSetGroupId = 2,
                    type = 5,
                    maxlength = 50,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "E001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 4,
                    key = "storeMemo",
                    name = "結帳備註",
                    memo = "可以輸入一段話，在結帳的時候對客戶做一些小提醒。",
                    FK_StoreSetGroupId = 2,
                    type = 2,
                    maxlength = 300,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "E001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 5,
                    key = "GTM",
                    name = "Google Tag Manager",
                    memo = "請輸入GOOGLE提供之驗證碼：GTM-xxxxxxx",
                    FK_StoreSetGroupId = 1,
                    type = 1,
                    maxlength = 12,
                    pattern = "^GTM-\\w+",
                    IsDeleted = false,
                    jobID = "S001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 6,
                    key = "linkMore",
                    name = "了解更多",
                    memo = "輸入一段連結，在商品頁中可以顯示了解更多按鈕。",
                    FK_StoreSetGroupId = 2,
                    type = 1,
                    maxlength = 255,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "E001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 7,
                    key = "prodCatalog",
                    name = "商品目錄",
                    memo = "輸入商品目錄連結，以利前台新增返回目錄按鈕。",
                    FK_StoreSetGroupId = 2,
                    type = 1,
                    maxlength = 255,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "E001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 11, 12, 11, 59, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 8,
                    key = "membershipTerms",
                    name = "會員條款",
                    memo = "請輸入會員條款內文",
                    FK_StoreSetGroupId = 2,
                    type = 2,
                    maxlength = 5000,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "E001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 11, 12, 11, 59, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );
            modelBuilder.Entity<storeSetItem>().HasData(
                new storeSetItem
                {
                    Id = 1,
                    Key = "zh-TW",
                    Value = "中文(繁體)",
                    FK_StoreSetId = 2,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 2,
                    Key = "zh-CN",
                    Value = "中文(簡體)",
                    FK_StoreSetId = 2,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 3,
                    Key = "en",
                    Value = "英文",
                    FK_StoreSetId = 2,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 4,
                    Key = "ja",
                    Value = "日文",
                    FK_StoreSetId = 2,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 5,
                    Key = "noPayNoShow",
                    Value = "不開放購物且不顯示商品售價",
                    FK_StoreSetId = 3,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 6,
                    Key = "noPay",
                    Value = "不開放購物但顯示商品售價",
                    FK_StoreSetId = 3,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 7,
                    Key = "menberPay",
                    Value = "限制僅會員購物",
                    FK_StoreSetId = 3,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 8,
                    Key = "Pay",
                    Value = "開放購物",
                    FK_StoreSetId = 3,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    Name = "系統總管理者",
                    Type = 0,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );
            modelBuilder.Entity<MappingUserAndRole>().HasData(
                new MappingUserAndRole
                {
                    Id = 1,
                    RoleId = 1,
                    UserId = 1,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 2, 1, 18, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );

            //金流
            modelBuilder.Entity<ThirdParty>().HasData(
                new ThirdParty
                {
                    Id = 1,
                    Title = "轉帳",
                    IsDeleted = false,
                    ser_no = 500,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new ThirdParty
                {
                    Id = 2,
                    Title = "支付連",
                    IsDeleted = false,
                    ser_no = 500,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new ThirdParty
                {
                    Id = 3,
                    Title = "LINE Pay",
                    IsDeleted = false,
                    ser_no = 500,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );
            modelBuilder.Entity<ThirdPartyKeypair>().HasData(
                new ThirdPartyKeypair
                {
                    Id = 1,
                    FK_TPid = 1,
                    Title = "匯款銀行代號",
                    Code = "bankNo",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ThirdPartyKeypair
                {
                    Id = 2,
                    FK_TPid = 1,
                    Title = "匯款帳號",
                    Code = "account",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ThirdPartyKeypair
                {
                    Id = 3,
                    FK_TPid = 1,
                    Title = "戶名",
                    Code = "shopID",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ThirdPartyKeypair
                {
                    Id = 4,
                    FK_TPid = 2,
                    Title = "PchomePayAppId",
                    Code = "account",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ThirdPartyKeypair
                {
                    Id = 5,
                    FK_TPid = 2,
                    Title = "PchomePaySecre",
                    Code = "code1",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ThirdPartyKeypair
                {
                    Id = 6,
                    FK_TPid = 2,
                    Title = "允許繳費有效天數",
                    Code = "expire_day2",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ThirdPartyKeypair
                {
                    Id = 7,
                    FK_TPid = 3,
                    Title = "Channel ID",
                    Code = "account",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new ThirdPartyKeypair
                {
                    Id = 8,
                    FK_TPid = 3,
                    Title = "Channel Secret Key",
                    Code = "code1",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }
            );
            modelBuilder.Entity<PaymentType>().HasData(
                new PaymentType
                {
                    Id = 1,
                    Used = false,
                    Title = "ATM",
                    Code = "atm",
                    SerNo = 1,
                    FK_ThirdPartyId = 1,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 2,
                    Used = false,
                    Title = "信用卡付款",
                    Code = "PchomePayCARD",
                    SerNo = 3,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 3,
                    Used = false,
                    Title = "ATM(虛擬帳戶)",
                    Code = "PchomePayATM",
                    SerNo = 8,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 4,
                    Used = false,
                    Title = "PI錢包付款",
                    Code = "PchomePayPI",
                    SerNo = 7,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 5,
                    Used = false,
                    Title = "支付連餘額付款",
                    Code = "PchomePayACCT",
                    SerNo = 500,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    IsDeleted = true,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 6,
                    Used = false,
                    Title = "支付連銀行支付付款",
                    Code = "PchomePayEACH",
                    SerNo = 9,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 7,
                    Used = false,
                    Title = "7-11貨到付款",
                    Code = "PCHomeIPL7",
                    SerNo = 10,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 8,
                    Used = false,
                    Title = "全家貨到付款",
                    Code = "PCHomeIPLFM",
                    SerNo = 11,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 9,
                    Used = false,
                    Title = "OK貨到付款",
                    Code = "PCHomeIPLOK",
                    SerNo = 500,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    IsDeleted = true,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 10,
                    Used = false,
                    Title = "萊爾富貨到付款",
                    Code = "PCHomeIPLHL",
                    SerNo = 12,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 11,
                    Used = false,
                    Title = "線上刷卡3期分期付款",
                    Code = "PchomePayInstallment3",
                    SerNo = 4,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 12,
                    Used = false,
                    Title = "線上刷卡6期分期付款",
                    Code = "PchomePayInstallment6",
                    SerNo = 5,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 13,
                    Used = false,
                    Title = "線上刷卡12期分期付款",
                    Code = "PchomePayInstallment12",
                    SerNo = 6,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 14,
                    Used = false,
                    Title = "LINEPay",
                    Code = "LinePay",
                    SerNo = 2,
                    FK_ThirdPartyId = 3,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 25, 19, 25, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }, new PaymentType
                {
                    Id = 15,
                    Used = false,
                    Title = "超商條碼付款",
                    Code = "PCHomeIBRCD",
                    SerNo = 13,
                    FK_ThirdPartyId = 2,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 11, 21, 14, 00, 00, 00, DateTimeKind.Local).AddTicks(1459),
                }
            );
        }
    }
}
