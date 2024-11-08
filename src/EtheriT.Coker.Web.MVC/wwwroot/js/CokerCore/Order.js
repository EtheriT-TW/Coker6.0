Coker.extend({
    Order: {
        GetHeader: function (id) {
            return $.ajax({
                url: "/api/Order/GetHeaderOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        GetDetails: function (id) {
            return $.ajax({
                url: "/api/Order/GetOrderDetails/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                data: { id: id },
            });
        },
        SendMail: function (id) {
            return $.ajax({
                url: "/api/Order/SendMail/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        },
        UpdateStatus: function (data) {
            return $.ajax({
                url: "/api/Order/UpdateStatus",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        GetOrderStatusLookup: function () {
            return $.ajax({
                url: "/api/Order/getOrderStatusLookup/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
            });
        }
    }
});