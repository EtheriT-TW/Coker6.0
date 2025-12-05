Coker.extend({
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
        getAllList: function () {
            return $.ajax({
                url: "/api/WebMenu/GetAllList/",
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
                type: "Delete",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ id: id }),
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
        },
        SetVisible: function (id, visible) {
            return $.ajax({
                url: "/api/WebMenu/SetVisible",
                type: "Post",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify({ Id: id, IsVisible: visible }),
                dataType: "json"
            });
        },
        GetPageTypeList: function () {
            return $.ajax({
                url: "/api/WebMenu/GetPageTypeList",
                type: "Get",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                dataType: "json"
            });
        },
    }
});