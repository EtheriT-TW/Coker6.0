var MinutesSecond = 60 * 1000;
var Coker = {
    Data: {
        DefauleUrl: "/Dashboard/index",
        Header: {
            Authorization: 'Bearer ' + $.cookie("token"),
            Secret: $.cookie("secret")
        },
        Time: {
            DataRetentionTime: 30 * MinutesSecond,
            ReCheckTime: 20 * MinutesSecond
        }
    },
    Cookie: {
        Add: function (key, value) {
            var expDate = new Date();
            expDate.setTime(expDate.getTime() + _c.Data.Time.DataRetentionTime);
            $.cookie(key, value, { path: "/", expires: expDate });
        },
        AddAll: function (obj) {
            for (var key in obj) {
                if (typeof (key) != "object") _c.Cookie.Add(key, obj[key]);
            }
        },
        Del: function (key) {
            $.removeCookie(key, { path: "/" });
        },
        Get: function (key) {
            return $.cookie(key);
        },
        DelAll: function () {
            var cookies = $.cookie();
            for (var cookie in cookies) {
                $.removeCookie(cookie, { path: "/" });
            }
        }
    },
    Page: {
        Ready: function () {
            if (location.pathname != "/") co.Cookie.Add("lastViewPage", location.pathname);
            typeof (PageReady) === "function" && PageReady();
        }
    },
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
                    token: result.token,
                    secret: result.secret,
                    endDateTime: (new Date(result.endDateTime)).getTime()
                });
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
            });
            return _dfr.promise();
        },
        Check: function () {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/User/Chech",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            }).done(function (result) {
                co.Cookie.AddAll({
                    token: result.token,
                    secret: result.secret,
                    endDateTime: (new Date(result.endDateTime)).getTime()
                });
                _dfr.resolve(result);
            }).fail(function () {
                _c.Cookie.DelAll();
                _dfr.resolve();
            });
            return _dfr.promise();
        }
    },
    Picker: {
        Init: function ($picker) {
            $picker.daterangepicker({
                timePicker: true,
                timePicker24Hour: true,
                autoUpdateInput: true,
                locale: {
                    format: 'YYYY/mm/DD HH:mm',
                    separator: " ~ ",
                    applyLabel: "　確認　",
                    cancelLabel: "　取消　",
                    daysOfWeek: ["日", "一", "二", "三", "四", "五", "六"],
                    monthNames: ["一月", "二月", "三月", "四月", "五月", "六月", "七月", "八月", "九月", "十月", "十一月", "十二月"]
                }
            });

            $picker.on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
            });
        },
    }
}
var _c = Coker;
var co = Coker;