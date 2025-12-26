using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EtheriT.Coker.EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class Alter_StoreSet_DefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultValue",
                table: "StoreSet",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 1L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 2L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 3L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 4L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 5L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 6L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 7L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 8L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 9L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 10L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 11L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 12L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 13L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 14L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 15L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 16L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 17L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 18L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 19L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 20L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 21L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 22L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 23L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 24L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 25L,
                column: "DefaultValue",
                value: null);

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 26L,
                columns: new[] { "DefaultValue", "memo" },
                values: new object[] { "關於電子郵件資料的使用說明\r\n\r\n尊敬的用戶，感謝您使用我們的服務。\r\n在使用第三方登入（如 Line 登入）時，我們會從您的帳號中取得您所提供的電子郵件地址。\r\n以下為我們使用電子郵件資料的說明：\r\n\r\n**1. 身分驗證**\r\n我們會使用您的電子郵件來確認您的身分，確保您在本平台上的登入狀態及安全性。\r\n當您使用電子郵件進行登入或註冊時，這些資料將會用於身分確認。\r\n\r\n**2. 忘記密碼通知信**\r\n當您忘記密碼並請求重設時，我們會將重設密碼的通知與相關說明寄送至您註冊時所提供的電子郵件地址，\r\n以協助您找回帳號的使用權限。\r\n\r\n**3. 購物通知信**\r\n在您進行購物時，若有訂單處理進度、商品出貨等相關狀況，\r\n我們會使用您的電子郵件地址向您發送通知，以便您隨時掌握購物狀態。\r\n\r\n**4. 付款成功通知信**\r\n當您完成付款後，系統將寄送付款成功通知信至您的電子郵件，\r\n以利您確認交易是否成功並保存交易紀錄。\r\n\r\n**5. 客服聯繫**\r\n當您與客服團隊聯繫時，我們會透過電子郵件回覆您的問題、提供協助，\r\n並處理相關客戶服務事宜。\r\n\r\n**隱私與資料保護說明**\r\n我們將妥善保護您的電子郵件資料，不會將其提供給第三方，\r\n除非基於法律要求或經您同意。\r\n您的電子郵件資料僅會用於上述用途，並依據隱私政策進行保護。\r\n\r\n如您對上述內容有任何疑問，歡迎隨時與我們聯繫。", "請輸入隱私聲明內文" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultValue",
                table: "StoreSet");

            migrationBuilder.UpdateData(
                table: "StoreSet",
                keyColumn: "Id",
                keyValue: 26L,
                column: "memo",
                value: "請輸入會員條款內文");
        }
    }
}
