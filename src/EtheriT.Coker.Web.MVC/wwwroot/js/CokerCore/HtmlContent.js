Coker.extend({
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
                data: { id: id }
            });
        },
        Delete: function (id) {
            const myId = parseInt(id);
            return $.ajax({
                url: "/api/HtmlContent/Delete",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: myId })
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
    }
});