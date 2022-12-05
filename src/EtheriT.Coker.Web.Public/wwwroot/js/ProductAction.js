var Product = {
    AddUp: {
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
    Update: {
        Cart: function (data) {
            return $.ajax({
                url: "/api/ShoppingCart/QuantityUpdate",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    },
    GetAll: {
        Cart: function (Tid) {
            return $.ajax({
                url: "/api/ShoppingCart/GetAll/",
                type: "GET",
                data: { Tid: Tid }
            });
        }
    },
    GetOne: {
        Cart: function (id) {
            return $.ajax({
                url: "/api/ShoppingCart/GetDropOne/",
                type: "GET",
                data: { id: id }
            });
        }
    },
    Delete: {
        Cart: function (id) {
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