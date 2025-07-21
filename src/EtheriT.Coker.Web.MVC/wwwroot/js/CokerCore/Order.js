/** @type {typeof orderModel} */
const orderModel = {
    GetDisplay: function (ohids) {
        return $.ajax({
            url: "/api/Order/GetOrderDisplay/",
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            headers: _c.Data.Header,
            data: { ohids: ohids },
        });
    },
    GetHeaderOld: function (id) {
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
    },
    GetPreserveTypeEnum: function () {
        return $.ajax({
            url: "/api/Order/GetPreserveTypeEnum",
            type: "POST",
            headers: _c.Data.Header
        });
    },
    GetShippingTypeEnum: function () {
        return $.ajax({
            url: "/api/Order/GetShippingTypeEnum",
            type: "POST",
            headers: _c.Data.Header
        });
    }
}
Coker.extend({
    Order: orderModel
});
co.Order = Coker.Order;