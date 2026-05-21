Coker.extend({
    Advertise: {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/Advertise/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        GetDataOne: function (Id) {
            return $.ajax({
                url: "/api/Advertise/GetDataOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        Delete: function (Id) {
            return $.ajax({
                url: "/api/Advertise/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        GetConten: function (data) {
            return $.ajax({
                url: "/api/Advertise/GetConten",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                headers: co.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        SaveConten: function (data) {
            return $.ajax({
                url: "/api/Advertise/SaveConten",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                headers: co.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        ImportConten: function (data) {
            return $.ajax({
                url: "/api/Advertise/ImportConten",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                headers: co.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    }
});