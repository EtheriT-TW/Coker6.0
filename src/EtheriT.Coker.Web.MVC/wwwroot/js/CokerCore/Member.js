Coker.extend({
    Member: {
        Get: function (id) {
            return $.ajax({
                url: "/api/Member/GetAllData/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id }
            });
        },
        Update: function (data) {
            return $.ajax({
                url: "/api/Member/Update",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        ForgetPassword: function (UserId) {
            return $.ajax({
                url: "/api/Member/ForgetPassword",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ UserId: UserId }),
                dataType: "json"
            });
        },
        GetSelf: function () {
            return $.ajax({
                url: "/api/Member/GetSelfData/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header
            });
        },
        GetAllRole: function () {
            return $.ajax({
                url: "/api/Member/GetAllRole/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header
            });
        },
        GetHistoryOrder: function (UUID) {
            return $.ajax({
                url: "/api/Order/GetMemberOrder/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { UUID: UUID }
            });
        },
        isValidPassword: function (password) {
            let typesCount = 0;
            if (/[a-z]/.test(password)) typesCount++; // 小寫字母
            if (/[A-Z]/.test(password)) typesCount++; // 大寫字母
            if (/\d/.test(password)) typesCount++;    // 數字
            if (/\W|_/.test(password)) typesCount++;  // 符號
            return typesCount >= 3 && password.length >= 8 && password.length <= 30;
        }
    }
});