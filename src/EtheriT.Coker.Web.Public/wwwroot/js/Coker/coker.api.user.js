(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});

    Coker.extend({
        User: {

            /** 新增會員 */
            AddUser: function (data) {
                return Coker.api.post("/api/User/AddUser", data);
            },

            /** 開通帳號 */
            AccountOpening: function (OpenId) {
                return Coker.api.get("/api/User/AccountOpening/", {
                    OpenId: OpenId
                });
            },

            /** 重送開通信 */
            AccountReSendOpening: function (data) {
                return Coker.api.post("/api/User/ReSendOpening", data);
            },

            /** 忘記密碼 */
            PasswordForget: function (data) {
                return Coker.api.post("/api/User/PasswordForget", data);
            },

            /** 忘記帳號檢查 */
            ForgetIdCheck: function (ForgetId) {
                return Coker.api.get("/api/User/ForgetIdCheck/", {
                    ForgetId: ForgetId
                });
            },

            /** 變更密碼 */
            PasswordChange: function (data) {
                return Coker.api.post("/api/User/PasswordChage", data);
            },

            /** 變更 Email */
            EmailChange: function (data) {
                return Coker.api.post("/api/User/EmailChage", data);
            },

            /** 登入 */
            Login: function (data) {
                // 原本 Login 沒帶 Authorization，保留行為
                return Coker.api.post("/api/User/Login", data, { auth: false });
            },

            /** 取得目前會員資料 */
            GetUser: function () {
                return Coker.api.get("/api/User/GetUserData/");
            },

            /** 前台會員資料編輯 */
            UserEdit: function (data) {
                return Coker.api.post("/api/User/FrontUserEdit", data);
            },

            /** 登出 */
            Logout: function () {
                return Coker.api.get("/api/User/Logout");
            }

        }
    });

})(window);