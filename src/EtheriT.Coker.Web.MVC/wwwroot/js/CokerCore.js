var MinutesSecond = 60 * 1000;
var MonthSecond = 30 * 24 * 60 * 60 * 1000;
var Coker = {
    Data: {
        DefauleUrl: "/Dashboard/index",
        Header: {
            Authorization: 'Bearer ' + $.cookie("token"),
            Secret: $.cookie("secret")
        },
        Time: {
            DataRetentionTime: 30 * MinutesSecond,
            DataRetentionLongTime: 3 * MonthSecond,
            ReCheckTime: 20 * MinutesSecond
        },
        HtmlDecode: function (str) {
            var _h = $("<div />").html(str).text();
            if (/[&].*[;]/.test(_h)) return _c.Data.HtmlDecode(_h);
            else return _h;
        }
    },
    Cookie: {
        EffectiveTime : 0,
        Add: function (key, value) {
            var expDate = new Date();
            expDate.setTime(expDate.getTime() + co.Cookie.EffectiveTime);
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
                if (cookie != "LastWebSite") $.removeCookie(cookie, { path: "/" });
            }
        }
    },
    Page: {
        Ready: function () {
            if (location.pathname != "/") co.Cookie.Add("lastViewPage", location.pathname);
            typeof (PageReady) === "function" && PageReady();
        }
    },
    WebSite: {
        exchange: function (id) {
            var _dfr = $.Deferred();
            $.ajax({
                url: "/api/Website/Exchange",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: id }),
                dataType: "json"
            }).done(function (result) {
                if (result.success) {
                    co.Cookie.EffectiveTime = co.Data.Time.DataRetentionLongTime;
                    co.Cookie.Add("LastWebSite", result.message);
                    co.Cookie.EffectiveTime = co.Data.Time.D;
                }
                _dfr.resolve(result);
            });
            return _dfr.promise();
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
                co.Cookie.EffectiveTime = co.Data.Time.DataRetentionLongTime;
                co.Cookie.Add("LastWebSite", co.Cookie.Get("LastWebSite"));
                co.Cookie.EffectiveTime = co.Data.Time.D;
                _dfr.resolve(result);
            }).fail(function () {
                _c.Cookie.DelAll();
                _dfr.resolve();
            });
            return _dfr.promise();
        }
    },
    sweet: {
        config: {
            timeout: 1500
        },
        success: function (text, action, autoclose) {
            var closetime = false;
            if (autoclose) { closetime = Coker.sweet.config.timeout }

            Swal.fire({
                icon: 'success',
                html: text,
                showConfirmButton: !autoclose,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                timer: closetime
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        error: function (title, text, action, autoclose) {
            var closetime = false;
            if (autoclose) { closetime = Coker.sweet.config.timeout }

            Swal.fire({
                icon: 'error',
                title: title,
                html: text,
                showConfirmButton: !autoclose,
                showCancelButton: false,
                confirmButtonColor: '#3085d6',
                confirmButtonText: '確定',
                reverseButtons: true,
                timer: closetime
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        confirm: function (title, text, confirmtexet, cancanceltext, action) {
            Swal.fire({
                icon: 'info',
                title: title,
                html: text,
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: confirmtexet,
                cancelButtonText: cancanceltext,
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    typeof (action) === "function" && action();
                }
            })
        },
        TitleHilight: function (string,title) {
            return string.replace("{0}", `<span class='ConfirmKeyWord'>${title}</span>`);
        }
    },
    Picker: {
        Init: function ($picker) {
            $picker.daterangepicker({
                timePicker: true,
                timePicker24Hour: true,
                autoUpdateInput: true,
                locale: {
                    format: 'YYYY/MM/DD HH:mm',
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
    },
    HtmlContent: {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/HtmlContent/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/HtmlContent/GetOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Delete: function (id) {
            return $.ajax({
                url: "/api/HtmlContent/Delete",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        },
        GetTypeList: function () {
            return $.ajax({
                url: "/api/HtmlContent/GetTypeList",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        GetAllComponent: function () {
            return $.ajax({
                url: "/api/HtmlContent/GetAllComponent",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        GetComponent: function (type) {
            return $.ajax({
                url: "/api/HtmlContent/GetComponent",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ type: type }),
                dataType: "json"
            });
        }
    },
    WebMesnus: {
        getAll: function () {
            return $.ajax({
                url: "/api/WebMenu/GetAll",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
        createOrEdit: function (data) {
            return $.ajax({
                url: "/api/WebMenu/CreateOrEdit",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        getConten: function (id) {
            return $.ajax({
                url: "/api/WebMenu/GetConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json"
            });
        },
        saveConten: function (data) {
            return $.ajax({
                url: "/api/WebMenu/saveConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        importConten: function (data) {
            return $.ajax({
                url: "/api/WebMenu/importConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        delete: function (id) {
            return $.ajax({
                url: "/api/WebMenu/Delete",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({id:id}),
                dataType: "json"
            });
        },
        updateLevelAndSerNo: function (list) {
            return $.ajax({
                url: "/api/WebMenu/updateLevelAndSerNo",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ list: list }),
                dataType: "json"
            });
        }
    }
}
var _c = Coker;
var co = Coker;
co.Cookie.EffectiveTime = co.Data.Time.DataRetentionTime;