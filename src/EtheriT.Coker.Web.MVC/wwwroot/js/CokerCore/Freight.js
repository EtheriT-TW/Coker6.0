const freightModel = {
    AddUp: function (data) {
        return $.ajax({
            url: "/api/Freight/AddUp",
            type: "POST",
            contentType: 'application/json; charset=utf-8',
            headers: _c.Data.Header,
            data: JSON.stringify(data),
            dataType: "json"
        });
    },
    Get: function (id) {
        return $.ajax({
            url: "/api/Freight/GetOne/",
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            headers: _c.Data.Header,
            data: { id: id },
        });
    },
    Delete: function (id) {
        return $.ajax({
            url: "/api/Freight/Delete/",
            type: "GET",
            contentType: 'application/json; charset=utf-8',
            headers: _c.Data.Header,
            data: { id: id },
        });
    }
}
Coker.extend({
    Freight: freightModel
});
/** @type {typeof freightModel} */
co.Freight;