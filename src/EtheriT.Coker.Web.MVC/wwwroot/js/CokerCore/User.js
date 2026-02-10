Coker.extend({
    User: {
        Login: function (para) {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/Login",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(para),
                dataType: "json"
            }).done(function (result) {
                co.Cookie.AddAll({
                    isLogin: true,
                    endDateTime: (new Date(result.endDateTime)).getTime()
                });
                _c.Data.Header.Authorization = 'Bearer ' + result.token;
                _c.Data.Header.Secret = result.secret;
                _dfr.resolve(result);
            });
            return _dfr.promise();
        },
        Logout: function () {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/Logout",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            }).done(function (result) {
                if (result.success) {
                    _c.Cookie.DelAll();
                    location.href = "/";
                    _dfr.resolve();
                }
            }).fail(function () {
                _c.Cookie.DelAll();
                location.href = "/";
            });
            return _dfr.promise();
        },
        Check: function () {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/Chech",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                dataType: "json"
            }).done(function (result) {
                if (!result.success) {
                    co.Cookie.AddAll({
                        isLogin: false
                    });
                    co.Cookie.Del("endDateTime");
                    _dfr.resolve(result);
                } else {
                    co.Cookie.AddAll({
                        isLogin: true,
                        endDateTime: (new Date(result.endDateTime)).getTime()
                    });
                    _c.Data.Header.Authorization = 'Bearer ' + result.token;
                    _c.Data.Header.Secret = result.secret;
                    _dfr.resolve(result);
                }
            }).fail(function () {
                _c.Cookie.DelAll();
                co.sweet.error("連線失敗", "延遲時間過久，您的登入狀態已被登出，請重新登入。", function () {
                    location.href = "/";
                }, false);
                _dfr.resolve();
            });
            return _dfr.promise();
        },
        UpdatePassword: function (para) {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/UpdatePassword",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(para),
                dataType: "json"
            }).done(function (result) {
                if (result.success) {
                    _c.sweet.success("密碼變更成功");
                    _dfr.resolve();
                } else {
                    _c.sweet.error(result.message, "密碼請包含英文大小寫、數字及特殊符號且密碼長度達十二碼以上!!");
                    _dfr.resolve();
                }
            });
            return _dfr.promise();
        }
    }
});