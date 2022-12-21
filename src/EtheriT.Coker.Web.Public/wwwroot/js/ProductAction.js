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
        },
        Prod: function (id) {
            return $.ajax({
                url: "/api/Product/GetDisplayOne/",
                type: "GET",
                data: { id: id }
            });
        },
        Stock: function (id) {
            return $.ajax({
                url: "/api/Product/GetDisplayStock/",
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
    },
    Log: {
        Click: function (data) {
            return $.ajax({
                url: "/api/Product/ClickLog",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    }
}