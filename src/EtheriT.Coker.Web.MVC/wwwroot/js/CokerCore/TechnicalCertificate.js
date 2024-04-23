Coker.extend({
    TechnicalCertificate: {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/TechnicalCertificate/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (id) {
            return $.ajax({
                url: "/api/TechnicalCertificate/GetOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { id: id },
            });
        },
        Delete: function (id) {
            return $.ajax({
                url: "/api/TechnicalCertificate/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: id },
            });
        },
        GetConten: function (data) {
            return $.ajax({
                url: "/api/TechnicalCertificate/GetConten/",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
            });
        }, SaveConten: function (data) {
            return $.ajax({
                url: "/api/TechnicalCertificate/SaveConten",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
    }
});