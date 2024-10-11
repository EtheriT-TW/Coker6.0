var Product = {
    AddUp: {
        Cart: function (data) {
            return $.ajax({
                url: "/api/ShoppingCart/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
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
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    },
    GetAll: {
        Cart: function (Tid) {
            return $.ajax({
                url: "/api/ShoppingCart/GetAll/",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
                type: "GET",
                data: { Tid: Tid }
            });
        }
    },
    GetOne: {
        Cart: function (id) {
            return $.ajax({
                url: "/api/ShoppingCart/GetDropOne/",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
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
        ProdMainDisplay: function (Id) {
            return $.ajax({
                url: "/api/Product/GetMainDisplayOne/",
                type: "GET",
                data: { Id: Id }
            });
        },
        ProdOne: function (id) {
            return $.ajax({
                url: "/api/Product/GetProdDataOne/",
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
        },
    },
    Delete: {
        Cart: function (id) {
            return $.ajax({
                url: "/api/ShoppingCart/DeleteDrop/",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("token")
                },
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