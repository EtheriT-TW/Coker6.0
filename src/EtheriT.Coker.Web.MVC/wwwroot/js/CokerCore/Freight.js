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
const logisticsBox = {
    Get: function (id) {
        return co.request.get("/api/Freight/LogisticsBoxGetOne", { id: id });
    },
    AddUp: function (dto) {
        return co.request.post("/api/Freight/LogisticsBoxAddUp", dto);
    },
    Delete: function (id) {
        return co.request.delete("/api/Freight/LogisticsBoxDelete", { id: id });
    },
    Requires: function () {
        return co.request.get("/api/Freight/LogisticsBoxRequires");
    }
};
Coker.extend({
    Freight: freightModel,
    LogisticsBox: logisticsBox
});
/** @type {typeof freightModel} */
co.Freight;