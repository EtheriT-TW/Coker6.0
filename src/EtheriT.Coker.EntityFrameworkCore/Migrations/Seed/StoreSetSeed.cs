using EtheriT.Coker.Application.Shared.Dto.enumType;
using EtheriT.Coker.Application.Shared.Dto.enumType.Member;
using EtheriT.Coker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EtheriT.Coker.EntityFrameworkCore.Migrations.Seed
{
    public static class StoreSetSeed
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
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
                }, new StoreSetGroup
                {
                    Id = 3,
                    Title = "信件伺服器設定",
                    Image = "",
                    Description = "",
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 12, 5, 18, 00, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSetGroup
                {
                    Id = 4,
                    Title = "版型設定",
                    Image = "",
                    Description = "",
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 12, 5, 18, 00, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSetGroup
                {
                    Id = 5,
                    Title = "其他設定",
                    Image = "",
                    Description = "",
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2025, 03, 28, 18, 00, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSetGroup
                {
                    Id = 6,
                    Title = "紅利設定",
                    Image = "",
                    Description = "",
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2025, 05, 07, 17, 07, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSetGroup
                {
                    Id = 7,
                    Title = "會員設定",
                    Image = "",
                    Description = "",
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2025, 12, 16, 17, 07, 00, 00, DateTimeKind.Local).AddTicks(1459)
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
                    type = SeoSetDataTypeEnum.text,
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
                    memo = "請選擇需要翻譯的語系（請洽詢客服加購功能）",
                    FK_StoreSetGroupId = 1,
                    type = SeoSetDataTypeEnum.checkBox,
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
                    type = SeoSetDataTypeEnum.select,
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
                    type = SeoSetDataTypeEnum.textarea,
                    Level = WebsiteLevelEnum.購物,
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
                    type = SeoSetDataTypeEnum.text,
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
                    type = SeoSetDataTypeEnum.text,
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
                    type = SeoSetDataTypeEnum.text,
                    Level = WebsiteLevelEnum.購物,
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
                    FK_StoreSetGroupId = 7,
                    type = SeoSetDataTypeEnum.textarea,
                    Level = WebsiteLevelEnum.會員,
                    maxlength = 5000,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "E001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 11, 12, 11, 59, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 9,
                    key = "SMTPPath",
                    name = "SMTP Server",
                    memo = "請輸入SMTP Server",
                    FK_StoreSetGroupId = 3,
                    type = SeoSetDataTypeEnum.text,
                    maxlength = 255,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "S001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 12, 05, 06, 00, 00, 00, DateTimeKind.Local)
                }, new StoreSet
                {
                    Id = 10,
                    key = "SMTPPort",
                    name = "Port",
                    memo = "請輸入Port",
                    FK_StoreSetGroupId = 3,
                    type = SeoSetDataTypeEnum.number,
                    maxlength = 5,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "S001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 12, 05, 06, 00, 00, 00, DateTimeKind.Local)
                }, new StoreSet
                {
                    Id = 11,
                    key = "SMTPAccount",
                    name = "帳號",
                    memo = "請輸入 SMTP 帳號；若不是 Email，系統將使用客服信箱作為寄件人",
                    FK_StoreSetGroupId = 3,
                    type = SeoSetDataTypeEnum.text,
                    maxlength = 100,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "S001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 12, 05, 06, 00, 00, 00, DateTimeKind.Local)
                }, new StoreSet
                {
                    Id = 12,
                    key = "SMTPPassword",
                    name = "密碼",
                    memo = "請輸入 密碼",
                    FK_StoreSetGroupId = 3,
                    type = SeoSetDataTypeEnum.password,
                    maxlength = 50,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "S001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 12, 05, 06, 00, 00, 00, DateTimeKind.Local)
                }, new StoreSet
                {
                    Id = 13,
                    key = "GoogleAds",
                    name = "Google Ads",
                    memo = "需先埋入GA，再輸入GOOGLE提供之轉換 ID：AW-xxxxxxxxxxx",
                    FK_StoreSetGroupId = 1,
                    type = SeoSetDataTypeEnum.text,
                    maxlength = 14,
                    pattern = "^AW-\\w+",
                    IsDeleted = false,
                    jobID = "S001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 14,
                    key = "NoCopy",
                    name = "鎖右鍵",
                    memo = "右鍵鎖定，文字圖片禁止圈選",
                    FK_StoreSetGroupId = 5,
                    type = SeoSetDataTypeEnum.checkBox,
                    maxlength = 14,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "Y001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 15,
                    key = "SignupBonusPoints",
                    name = "迎新禮",
                    memo = "加入會員贈送紅利點數",
                    FK_StoreSetGroupId = 6,
                    type = SeoSetDataTypeEnum.number,
                    maxlength = 8,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "B001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 16,
                    key = "MinOrderForRedemption",
                    name = "紅利扣抵條件",
                    memo = "單筆訂單消費滿足多少可使用紅利扣抵金額",
                    FK_StoreSetGroupId = 6,
                    type = SeoSetDataTypeEnum.number,
                    maxlength = 6,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "B001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 17,
                    key = "MaxRedemptionPercent",
                    name = "最高抵扣%",
                    memo = "單筆訂單抵扣%數上限",
                    FK_StoreSetGroupId = 6,
                    type = SeoSetDataTypeEnum.number,
                    maxlength = 2,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "B001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 18,
                    key = "MinOrderForEarnPoints",
                    name = "消費條件",
                    memo = "消費滿額多少金額贈送紅利回饋金",
                    FK_StoreSetGroupId = 6,
                    type = SeoSetDataTypeEnum.number,
                    maxlength = 8,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "B001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 19,
                    key = "RewardRatePercent",
                    name = "獲得%數紅利",
                    memo = "消費滿足條件贈送幾%紅利回饋金",
                    FK_StoreSetGroupId = 6,
                    type = SeoSetDataTypeEnum.number,
                    maxlength = 2,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "B001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 20,
                    key = "RewardPointsExpireDays",
                    name = "有效天數",
                    memo = "每一筆紅利的有效天數，如無須限制可不輸入",
                    FK_StoreSetGroupId = 6,
                    type = SeoSetDataTypeEnum.number,
                    maxlength = 3,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "B001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 21,
                    key = "priceOrder",
                    name = "價格顯示順序",
                    memo = "商品預設顯示的金額",
                    FK_StoreSetGroupId = 2,
                    type = SeoSetDataTypeEnum.select,
                    maxlength = 3,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "B001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2023, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 22,
                    key = "EmailNotificationType",
                    name = "信件寄送方式",
                    memo = "請選擇信件寄送方式",
                    FK_StoreSetGroupId = 3,
                    type = SeoSetDataTypeEnum.radio,
                    maxlength = 50,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "S001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 12, 05, 06, 00, 00, 00, DateTimeKind.Local)
                }, new StoreSet
                {
                    Id = 23,
                    key = "MemberRegister",
                    name = "開放註冊",
                    memo = "是否開放註冊，若關閉註冊僅可在會員清單新增。",
                    FK_StoreSetGroupId = 7,
                    type = SeoSetDataTypeEnum.select,
                    maxlength = null,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "M001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2025, 12, 16, 06, 00, 00, 00, DateTimeKind.Local)
                }, new StoreSet
                {
                    Id = 24,
                    key = "HasInvoice",
                    name = "開立發票",
                    memo = "網站是否需要開立發票",
                    FK_StoreSetGroupId = 2,
                    type = SeoSetDataTypeEnum.select,
                    maxlength = null,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "M001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2025, 12, 16, 06, 00, 00, 00, DateTimeKind.Local)
                }, new StoreSet
                {
                    Id = 25,
                    key = "ExtraInviiceCarrier",
                    name = "發票載具",
                    memo = "允許用戶使用發票載具類型。",
                    FK_StoreSetGroupId = 2,
                    type = SeoSetDataTypeEnum.checkBox,
                    maxlength = null,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "M001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2025, 12, 16, 06, 00, 00, 00, DateTimeKind.Local)
                }, new StoreSet
                {
                    Id = 26,
                    key = "PrivacyPolicy",
                    name = "隱私聲明",
                    memo = "請輸入隱私聲明內文(本區塊支援 Markdown 標記語法，### 表示標題，**文字** 表示字粗體)",
                    FK_StoreSetGroupId = 7,
                    type = SeoSetDataTypeEnum.textarea,
                    maxlength = 5000,
                    pattern = "",
                    IsDeleted = false,
                    DefaultValue = """
### 關於電子郵件資料的使用說明

尊敬的用戶，感謝您使用我們的服務。在使用第三方登入（如 Line 登入）時，我們會從您的帳號中取得您所提供的電子郵件地址。以下為我們使用電子郵件資料的說明：

**1. 身分驗證**
我們會使用您的電子郵件來確認您的身分，確保您在本平台上的登入狀態及安全性。當您使用電子郵件進行登入或註冊時，這些資料將會用於身分確認。

**2. 忘記密碼通知信**
當您忘記密碼並請求重設時，我們會將重設密碼的通知與相關說明寄送至您註冊時所提供的電子郵件地址，以協助您找回帳號的使用權限。

**3. 購物通知信**
在您進行購物時，若有訂單處理進度、商品出貨等相關狀況，我們會使用您的電子郵件地址向您發送通知，以便您隨時掌握購物狀態。

**4. 付款成功通知信**
當您完成付款後，系統將寄送付款成功通知信至您的電子郵件，以利您確認交易是否成功並保存交易紀錄。

**5. 客服聯繫**
當您與客服團隊聯繫時，我們會透過電子郵件回覆您的問題、提供協助，並處理相關客戶服務事宜。

**隱私與資料保護說明**
我們將妥善保護您的電子郵件資料，不會將其提供給第三方，除非基於法律要求或經您同意。您的電子郵件資料僅會用於上述用途，並依據隱私政策進行保護。

如您對上述內容有任何疑問，歡迎隨時與我們聯繫。
""",
                    jobID = "E001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2025, 12, 22, 14, 09, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new StoreSet
                {
                    Id = 27,
                    key = "BonusEnabled",
                    name = "紅利功能啟用",
                    memo = "是否啟用紅利功能",
                    FK_StoreSetGroupId = 6,
                    type = SeoSetDataTypeEnum.checkBox,
                    maxlength = 0,
                    pattern = "",
                    IsDeleted = false,
                    jobID = "B001",
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2026, 7, 25, 19, 08, 00, 00, DateTimeKind.Local).AddTicks(1459)
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
                    Level = WebsiteLevelEnum.購物,
                    FK_StoreSetId = 3,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 8,
                    Key = "Pay",
                    Value = "開放購物",
                    Level = WebsiteLevelEnum.購物,
                    FK_StoreSetId = 3,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 9,
                    Key = "HtoL",
                    Value = "由高至低",
                    Level = WebsiteLevelEnum.形象,
                    FK_StoreSetId = 21,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 10,
                    Key = "LtoH",
                    Value = "由低至高",
                    Level = WebsiteLevelEnum.形象,
                    FK_StoreSetId = 21,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 23, 14, 38, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 11,
                    Key = ((int)EmailNotificationTypeEnum.寄送完整表單).ToString(),
                    Value = EmailNotificationTypeEnum.寄送完整表單.ToString(),
                    FK_StoreSetId = 22,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 12,
                    Key = ((int)EmailNotificationTypeEnum.簡易通知).ToString(),
                    Value = EmailNotificationTypeEnum.簡易通知.ToString(),
                    FK_StoreSetId = 22,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 13,
                    Key = ((int)MemberRegisterTypeEnum.開放註冊).ToString(),
                    Value = MemberRegisterTypeEnum.開放註冊.ToString(),
                    FK_StoreSetId = 23,
                    IsDefault = true,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 14,
                    Key = ((int)MemberRegisterTypeEnum.關閉註冊).ToString(),
                    Value = MemberRegisterTypeEnum.關閉註冊.ToString(),
                    FK_StoreSetId = 23,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 15,
                    Key = "EnabledInvoice",
                    Value = "是",
                    FK_StoreSetId = 24,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 16,
                    Key = "DisabledInvoice",
                    Value = "否",
                    FK_StoreSetId = 24,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }, new storeSetItem
                {
                    Id = 17,
                    Key = "MobileCarrier",
                    Value = "手機載具",
                    FK_StoreSetId = 25,
                    IsDeleted = false,
                    CreatorUserId = 1,
                    CreationTime = new DateTime(2024, 7, 17, 18, 04, 00, 00, DateTimeKind.Local).AddTicks(1459)
                }
            );
        }
    }
}
