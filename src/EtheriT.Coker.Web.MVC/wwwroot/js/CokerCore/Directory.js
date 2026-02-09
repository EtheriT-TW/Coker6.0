Coker.extend({
    Directory: {
        AddUp: function (data) {
            return $.ajax({
                url: "/api/Directory/AddUp",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        },
        Get: function (Id) {
            return $.ajax({
                url: "/api/Directory/GetDataOne/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        Delete: function (Id) {
            return $.ajax({
                url: "/api/Directory/Delete/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: Id },
            });
        },
        GetDirectoryFacetConfig: function (DirectoryId) {
            return $.ajax({
                url: "/api/Directory/GetDirectoryFacetConfig/",
                type: "GET",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: { Id: DirectoryId },
            });
        },
        SaveDirectoryFacetConfig: function (data) {
            return $.ajax({
                url: "/api/Directory/SaveDirectoryFacetConfig",
                type: "POST",
                contentType: 'application/json; charset=utf-8',
                headers: _c.Data.Header,
                data: JSON.stringify(data),
                dataType: "json"
            });
        }
    }
});