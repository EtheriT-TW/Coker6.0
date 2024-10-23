Coker.extend({
    ObjectType: {
        GetAll: function () {
            return $.ajax({
                url: "/api/ObjectType/GetAll",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        }, createOrEdit: function (data) {
            return $.ajax({
                url: "/api/ObjectType/CreateOrEdit",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }, delete: function (id) {
            return $.ajax({
                url: "/api/ObjectType/DeleteHtmlContent",
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json"
            });
        }, updateSerNo: function (list) {
            return $.ajax({
                url: "/api/ObjectType/UpdateSerNo",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ list: list }),
                dataType: "json"
            });
        }, getConten: function (id) {
            return $.ajax({
                url: "/api/ObjectType/GetConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
                dataType: "json"
            });
        }, GetNewsletterConten: function () {
            return $.ajax({
                url: "/api/ObjectType/GetNewsletterConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        }, GetNewsletterAllConten: function () {
            return $.ajax({
                url: "/api/ObjectType/GetNewsletterAllConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        }, SaveConten: function (data) {
            return $.ajax({
                url: "/api/ObjectType/saveConten",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    }
});