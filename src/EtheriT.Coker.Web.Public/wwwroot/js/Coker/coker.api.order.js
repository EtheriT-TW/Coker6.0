(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        Order: {

            /** 建立訂單 Header */
            AddHeader: function (data) {
                return Coker.api.post("/api/Order/AddHeader", data);
            },

            /** 前台會員資料更新（訂單流程用） */
            FrontUserUpdate: function (data) {
                return Coker.api.post("/api/Order/FrontUserUpdate", data);
            },

            /** 取得單一訂單 Header */
            GetHeader: function (id) {
                return Coker.api.get("/api/Order/GetHeaderOne/", { id: id });
            },

            /** 取得訂單明細 */
            GetDetails: function (id) {
                return Coker.api.get("/api/Order/GetOrderDetails/", { id: id });
            },

            /** 取得前台訂單顯示資料；伺服器固定驗證目前會員或訪客工作階段 */
            GetAllData: function (ohid) {
                return Coker.api.get("/api/Order/GetOrderDisplay", {
                    ohid: ohid
                });
            },
            /** 取得會員歷史訂單 */
            GetHistoryOrder: function (page) {
                return Coker.api.get("/api/Order/GetHistoryOrder/", {
                    page: page
                });
            },

            /** 取得重新下單顯示資料 */
            GetReorder: function (ohid) {
                return Coker.api.get("/api/Order/ReorderDisplay", { ohid: ohid });
            },

            /** 付款方式 Enum（保留 POST 行為） */
            GetPaymentTypeEnum: function () {
                // 這支 API 非 JSON body，維持原始 ajax 行為較安全
                return $.ajax({
                    url: "/api/Order/GetPaymentTypeEnum",
                    type: "POST",
                    headers: Coker.api.authHeader()
                });
            },

            /** 檢查庫存 */
            CheckStock: function (data) {
                return Coker.api.post("/api/Order/CheckStock", data);
            },

            /** 重新下單 */
            Reorder: function (ohid) {
                return Coker.api.get("/api/Order/Reorder/", { ohid: ohid });
            },

            /** 取消訂單 */
            CancelOrder: function (ohid, payment) {
                return Coker.api.get("/api/Order/CancelOrder/", {
                    ohid: ohid,
                    payment: payment
                });
            },
        }
    });

})(window);
