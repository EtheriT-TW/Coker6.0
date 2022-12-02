var Product = {
    Add: {
        Cart: function (data) {
            return $.ajax({
                url: "/api/ShoppingCart/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    },
    Get: {
        DropCart: function (id) {
            return $.ajax({
                url: "/api/ShoppingCart/GetDrop/",
                type: "GET",
                data: { id: id }
            });
        }
    },
    Delete: {
        DropCart: function (id) {
            return $.ajax({
                url: "/api/ShoppingCart/DeleteDrop/",
                type: "GET",
                data: { id: id }
            });
        }
    }
    /*
     Coker.Order = {
        Add: function (data) {
            return $.ajax({
                url: "/api/Order/Add",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    };
     */

    //Cookie: {
    //    Add: function (key, value) {
    //        var expDate = new Date();
    //        expDate.setTime(expDate.getTime() + _c.Data.Time.DataRetentionTime);
    //        $.cookie(key, value, { path: "/", expires: expDate });
    //    },
    //    AddAll: function (obj) {
    //        for (var key in obj) {
    //            if (typeof (key) != "object") _c.Cookie.Add(key, obj[key]);
    //        }
    //    },
    //    Del: function (key) {
    //        $.removeCookie(key, { path: "/" });
    //    },
    //    Get: function (key) {
    //        return $.cookie(key);
    //    },
    //    DelAll: function () {
    //        var cookies = $.cookie();
    //        for (var cookie in cookies) {
    //            $.removeCookie(cookie, { path: "/" });
    //        }
    //    }
    //}
}